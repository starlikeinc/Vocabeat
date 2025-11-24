using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace LUIZ.InputSystem
{
    public enum EInputDeviceType
    {
        Unknown,
        
        Gamepad,        //게임패드 (XBox, PS, etc)
        Joystick,       //조이스틱
        XRController,   //XR (VR/AR) 컨트롤러
        
        Keyboard,       //키보드
        Mouse,          //마우스
        
        TouchScreen,    //터치스크린
        Pen,            //스타일러스 / 펜 입력
    }
    
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(EventSystem))]
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public abstract class ManagerInputBase : SingletonBase<ManagerInputBase>
    {
        [SerializeField] private InputSO PlayerInputSO; 

        public InputSO GetInputChannel => PlayerInputSO;
        
        private PlayerInput m_playerInput;
        private InputSystemUIInputModule m_uiInputModule;
        
        private IInputActionCollection2 m_inputActions;
        private InputActionAsset m_inputActionAsset;

        private InputDevice m_lastUsedDevice  = null;
        private InputActionRebindingExtensions.RebindingOperation currentRebindOperation;
        
        //----------------------------------------------------------------
        public bool IsRebindingInProgress => currentRebindOperation != null;
        
        //----------------------------------------------------------------
        protected override void OnUnityAwake()
        {
            base.OnUnityAwake();
            
            m_playerInput = GetComponent<PlayerInput>();
            m_uiInputModule = GetComponent<InputSystemUIInputModule>();
        }
        
        protected override void OnUnityEnable()
        {
            base.OnUnityEnable();          
            UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceConnectionEvent;
            m_playerInput.onControlsChanged += OnLastInputDeviceChange;
        }

        protected override void OnUnityDisable()
        {
            base.OnUnityDisable();
            UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceConnectionEvent;
            m_playerInput.onControlsChanged -= OnLastInputDeviceChange;
        }
        
        //----------------------------------------------------------------
        private void Update()
        {
/*#if UNITY_EDITOR //Editor-only 스크립트 작성한 후에 거기서 돌릴것
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F9))
                DoDebugPrintEnabledActionMaps();
#endif*/
            PlayerInputSO.InterUpdatePolling();
        }

        //----------------------------------------------------------------
        protected void ProtMgrInputBaseSetting(IInputActionCollection2 inputActions, InputActionAsset inputActionAsset)
        {
            m_inputActions = inputActions;
            m_inputActionAsset = inputActionAsset;
            
            PlayerInputSO.Init(ResolveAction);
            //DisableAllActionMaps();
        }
        private InputAction ResolveAction(string actionName) => m_inputActions.FindAction(actionName);
        
        //최초 세팅
        protected void ProtMgrInputAddAction<TReturnType>(string actionName) where TReturnType : struct
        {
            var inputAction = m_inputActions.FindAction(actionName);
            if (inputAction == null)
            {
                Debug.LogError($"[ManagerInputBase] {actionName} does not exist!!");
                return;
            }
            
            if (!inputAction.enabled)
            {
                inputAction.Enable();
                Debug.Log($"[ManagerInputBase] '{actionName}' was not enabled. Now enabled.");
            }
        }

        //----------------------------------------------------------------
        public void DoMgrInputBaseChangeActionMap(string mapName)
        {
            m_playerInput.SwitchCurrentActionMap(mapName);
            
            var map = m_playerInput.currentActionMap;
            if (map != null && !map.enabled)
                map.Enable();

            Debug.Log($"Current ActionMap: {m_playerInput.currentActionMap?.name}");
        }
        
        /// <summary> 모든 액션 맵을 비활성화. UI의 경우 InputSystemUIInputModule에서 따로 사용하기 때문에 이 함수를 호출해도 UI 입력은 막히지 않는다.
        /// 유아이 클릭도 막고싶으면 DoMgrInputBaseUIInputOnOff를 호출할것</summary>
        public void DoMgrInputBaseDisableAllActionMaps()
        {
            foreach (var map in m_inputActionAsset.actionMaps)
            {
                Debug.Log($"Disable ActionMap : {map.name}");
                map.Disable();
            }
        }

        public void DoMgrInputBaseUIInputOnOff(bool isBlockUIInput)
        {
            m_uiInputModule.enabled = !isBlockUIInput;
        }
        
        //특정 Action의 index에 해당하는 바인딩을 바꾼다.
        //타임 아웃 등은 유아이 쪽에서 알아서 핸들링 할것
        public async Task<(bool success, string bindingPath)> DoMgrInputBaseOverrideBindings(string actionName, int bindingIndex, Func<EInputDeviceType, bool> isControlValid = null)
        {
            #region ==========유효성 체크==========
            if (currentRebindOperation != null)
            {
                Debug.LogWarning("이미 리바인드 중입니다!");
                return(false, null);
            }

            var action = m_inputActions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogError($"Action '{actionName}'를 찾을 수 없습니다.");
                return(false, null);
            }

            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                Debug.LogError($"Binding index {bindingIndex}가 액션 '{actionName}'에 유효하지 않습니다.");
                return(false, null);
            }
            #endregion

            var tcs = new TaskCompletionSource<(bool, string)>();
            
            currentRebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                //.WithControlsExcluding("Mouse") // 필요하면 예외 컨트롤 설정
                .OnPotentialMatch(op =>
                {
                    var control = op.selectedControl;
                    var deviceType = PrivGetInputDeviceType(control.device);
                    if (isControlValid != null && !isControlValid(deviceType))
                    {
                        Debug.LogWarning($"[Rebinding] 입력된 장치가 유효하지 않음: {control.device}");
                        op.Cancel();
                    }
                })
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    currentRebindOperation = null;
                    var bindingPath = action.bindings[bindingIndex].effectivePath;// 변경된 바인딩 경로 넘겨줌
                    tcs.TrySetResult((true, bindingPath));
                })
                .OnCancel(operation =>
                {
                    operation.Dispose();
                    currentRebindOperation = null;
                    tcs.TrySetResult((false, null));
                })
                .Start();
            
            return await tcs.Task;
        }
        
        // 진행 중인 리바인드 취소 (필요하면)
        public void DoMgrInputBaseRebindingCancel()
        {
            if (currentRebindOperation == null)
                return;
            
            currentRebindOperation.Cancel();
            currentRebindOperation.Dispose();
            currentRebindOperation = null;
        }

        //바인딩 오버라이드 적용 (로컬에 json 형식등으로 저장된 바인딩을 불러온 후 해당 함수로 게임 가동 시 적용..)
        //Dictionary<액션이름,경로[바인딩 인덱스]>
        public void DoMgrInputBaseApplyBindings(Dictionary<string, string[]> dicBindings)
        {
            foreach (var kvp in dicBindings)
            {
                var action = m_inputActions.FindAction(kvp.Key);
                if (action == null) continue;

                action.RemoveAllBindingOverrides();

                var bindings = kvp.Value;
                for (int i = 0; i < bindings.Length; i++)
                {
                    if (i < action.bindings.Count)
                    {
                        if (!string.IsNullOrWhiteSpace(bindings[i]))
                            action.ApplyBindingOverride(i, bindings[i]);
                    }
                    else
                        Debug.LogWarning($"[ApplyBindingOverride] '{kvp.Key}' 액션에 잘못된 인덱스 {i}");
                }
            }
        }
        
        //현재 적용되고 있는 모든 바인딩 불러오기 (불러온 후 로컬에 json 형식등으로 저장 해둔다)
        //Dictionary<액션이름,경로[바인딩 인덱스]>
        //모바일 ActionMap 같은거도 같이 불러와지기 때문에 isMapNameIgnore를 통해 플랫폼 빌드 별로 잘 조정 할 것
        public void DoMgrInputBaseGetCurrentBindings(Dictionary<string, string[]> dic, Func<string, bool> isMapNameIgnore = null)
        {
            dic.Clear();
            
            foreach (var map in m_inputActionAsset.actionMaps)
            {
                if (isMapNameIgnore != null && isMapNameIgnore(map.name))
                    continue;

                foreach (var action in map.actions)
                {
                    var bindings = new string[action.bindings.Count];

                    for (int i = 0; i < bindings.Length; i++)
                    {
                        var b = action.bindings[i];
                        bindings[i] = !string.IsNullOrEmpty(b.overridePath) ? b.overridePath : b.path;
                    }

                    dic[action.name] = bindings;
                }
            }
        }
        
        //모든 액션의 바인딩 오버라이드를 초기화해서 기본값으로 되돌린다.( 저장은 알아서 해줄 것 )
        public void DoMgrInputBaseResetAllBindingOverrides()
        {
            foreach (var action in m_inputActions)
            {
                action.RemoveAllBindingOverrides();
            }
        }
        
        //action과 bindingPath(경로) 받아서 DisplayString 반환
        public string DoMgrInputBaseGetDisplayStringFromBindingPath(string actionName, string bindingPath)
        {
            var action = m_inputActions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"GetDisplayStringFromBindingPath: 액션 '{actionName}'을 찾을 수 없습니다.");
                return string.Empty;
            }

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                // 오버라이드 포함해서 경로가 일치하면
                if (string.Equals(binding.effectivePath, bindingPath, StringComparison.OrdinalIgnoreCase))
                {
                    return action.GetBindingDisplayString(i);
                }
            }

            Debug.LogWarning($"GetDisplayStringFromBindingPath: '{actionName}' 액션에서 bindingPath '{bindingPath}'를 찾을 수 없음.");
            return string.Empty;
        }
#if UNITY_EDITOR //유니티 전용 디버그
        public void DoDebugPrintEnabledActionMaps()
        {
            if (m_inputActionAsset == null)
            {
                Debug.LogWarning("[ManagerInputBase] InputActionAsset이 null입니다.");
                return;
            }

            Debug.Log("===== [ManagerInputBase] 현재 활성화된 ActionMap =====");
            foreach (var map in m_inputActionAsset.actionMaps)
            {
                string status = map.enabled ? "ENABLED" : "DISABLED";
                Debug.Log($" - {map.name}: {status}");
            }
            Debug.Log("===============================================");
        }
#endif    
        //------------------------------------------------------------------------------
        private EInputDeviceType PrivGetInputDeviceType(InputDevice device)
        {
            switch (device)
            {
                case Gamepad:
                    return EInputDeviceType.Gamepad;
                case Keyboard:
                    return EInputDeviceType.Keyboard;
                case Mouse:
                    return EInputDeviceType.Mouse;
                case Touchscreen:
                    return EInputDeviceType.TouchScreen;
                case UnityEngine.InputSystem.XR.XRController _:
                    return EInputDeviceType.XRController;
                case Joystick:
                    return EInputDeviceType.Joystick;
                case Pen _:
                    return EInputDeviceType.Pen;
                default:
                    return EInputDeviceType.Unknown;
            }
        }
        
        //------------------------------------------------------------------------------
        private void OnLastInputDeviceChange(PlayerInput playerInput)
        {
            if (m_lastUsedDevice != null)
            {
                var deviceType = PrivGetInputDeviceType(m_lastUsedDevice);
                PlayerInputSO?.InterInvokeLastInputDeviceChanged(deviceType);
                Debug.Log($"InputDevice Changed. Device : {deviceType.ToString()}");
            }
            else
            {
                Debug.LogWarning("[ManagerInputBase] 마지막 입력 장치가 아직 없습니다.");
                PlayerInputSO?.InterInvokeLastInputDeviceChanged(EInputDeviceType.Unknown);
            }
        }
        
        private void OnDeviceConnectionEvent(InputDevice device, InputDeviceChange change)
        { 
            var deviceType = PrivGetInputDeviceType(device);
            PlayerInputSO?.InterInvokeInputDeviceConnectionEvent(deviceType, change);
            Debug.Log($"InputDevice Connection Event. Device : {deviceType.ToString()}");
        }
    }
}
