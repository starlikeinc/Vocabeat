using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LUIZ.DataGraph.Editor
{
    public class DataGraphNodeBaseEditor : Node
    {
        public static int SnapSize = 10;
        
        private readonly CustomEdgeConnectorListener m_edgeConnectorListener;
        
        private readonly INodeHostContext m_nodeContext;
        private readonly DataGraphNodeBase m_graphNode;//실제 데이터
        private readonly List<Port> m_listPorts = new List<Port>();

		private string m_nodeInfoName;
		private string m_nodeInfoDescription;
		private VisualElement m_descriptionContainer;
        
		//---------------------------------------------------------------
        public DataGraphNodeBase Node => m_graphNode;
        public List<Port> Ports => m_listPorts;
        
		//---------------------------------------------------------------
        public DataGraphNodeBaseEditor(DataGraphNodeBase dataGraphNode, INodeHostContext context) 
        {
            AddToClassList("data-graph-node");

            m_graphNode = dataGraphNode;
            m_nodeContext = context;
            m_edgeConnectorListener = new CustomEdgeConnectorListener(context);
            
            //노드 배경 컬러 세팅
            Type type = dataGraphNode.GetType();
            NodeInfoAttribute nodeInfoAttribute = type.GetCustomAttribute<NodeInfoAttribute>();
            this.style.backgroundColor = new StyleColor(nodeInfoAttribute.DefaultColor);

            NodeTooltip nodeTooltipAttribute = type.GetCustomAttribute<NodeTooltip>();
            
            //기본 데이터
            title = nodeInfoAttribute.Title;
            name = type.Name;
            tooltip = nodeTooltipAttribute == null ? string.Empty : nodeTooltipAttribute.Content;

            string[] depths = nodeInfoAttribute.MenuItem.Split('/');
            foreach (string depth in depths)
                AddToClassList(depth.ToLower().Replace(" ", "-"));

            //노드에 포트 추가
            CreateAllPorts(nodeInfoAttribute);
            
			//NodeInfo 이름,설명 박스
			CreateNodeInfoNameDescription();
			
			//[ShowInNodeBody] 어트리뷰트 적용
			AddInlineFieldsFromAttributes();
			
			//서브그래프일 경우 더블 클릭 이벤트
			RegisterCallback<MouseDownEvent>(evt =>
			{
				if (evt.clickCount == 2)
					OnDoubleClick();
			});
        }

        //---------------------------------------------------------------
        public override void SetPosition(Rect newPos)
        {
            float snappedX = Mathf.Round(newPos.x / SnapSize) * SnapSize;
            float snappedY = Mathf.Round(newPos.y / SnapSize) * SnapSize;

            base.SetPosition(new Rect(snappedX, snappedY, newPos.width, newPos.height));
        }

        //---------------------------------------------------------------
        public void DoSavePosition()
        {
	        ((IDataGraphNode)m_graphNode).SetPosition(GetPosition());
        }

        //---------------------------------------------------------------
		private void OnDoubleClick()//더블클릭시
		{
            //TODO :
		}
		
		//---------------------------------------------------------------
        private void CreateAllPorts(NodeInfoAttribute nodeInfoAttribute)
        {
            if (m_graphNode is IOutputPortProvider outputProvider)
            {
                foreach (var portInfo in outputProvider.GetOutputPorts())
                    CreateTypedPort(Direction.Output, in portInfo, outputContainer);
            }
            if (m_graphNode is IDynamicOutputPortProvider)
                CreateAddDynamicPortButton(outputContainer, Direction.Output);
            
            if (m_graphNode is IInputPortProvider inputProvider)
            {
                foreach (var portInfo in inputProvider.GetInputPorts())
                    CreateTypedPort(Direction.Input, in portInfo, inputContainer);
            }
            if (m_graphNode is IDynamicInputPortProvider)
                CreateAddDynamicPortButton(inputContainer, Direction.Input);
        }

        private void CreateTypedPort(Direction direction, in PortDefinition portInfo, VisualElement container)
        {
            Port port = InstantiatePort(Orientation.Horizontal, direction, Port.Capacity.Multi, typeof(object));
            port.tooltip = $"AcceptType : {portInfo.AcceptedType?.Name ?? "None"}\nMin : {portInfo.MinConnectionCount}, Max : {GetMaxConnectionSymbol(portInfo.MaxConnectionCount)}";
            port.portName = portInfo.Name;
            PortMetaData newPortData = new PortMetaData(
                portInfo.Name,
                portInfo.PortID,
                portInfo.ChannelID,
                portInfo.AcceptedType,
                portInfo.MinConnectionCount,
                portInfo.MaxConnectionCount,
                portInfo.IsDynamic
            );
            port.userData = newPortData;
      
            //포트 연결기 등록
            var edgeConnector = new EdgeConnector<Edge>(m_edgeConnectorListener);
            port.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0)
                    m_edgeConnectorListener.OnStartEdgeDrag(port);
            });
            port.AddManipulator(edgeConnector);

            VisualElement portElement = port;
            if (portInfo.IsDynamic)
            {
                //배경색
                port.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f)); // RGB (217, 217, 217)
                //둥근 테두리
                port.style.borderTopLeftRadius = 4;
                port.style.borderTopRightRadius = 4;
                port.style.borderBottomLeftRadius = 4;
                port.style.borderBottomRightRadius = 4;
                
                var inputProvider = m_graphNode as IDynamicInputPortProvider;
                var outputProvider = m_graphNode as IDynamicOutputPortProvider;
                
                bool isDynamicInput = direction == Direction.Input && inputProvider != null;
                bool isDynamicOutput = direction == Direction.Output && outputProvider != null;

                int dynamicCount = 0;
                if (isDynamicInput)
                    dynamicCount = inputProvider.GetInputPorts().Count(p => p.IsDynamic);
                else if (isDynamicOutput)
                    dynamicCount = outputProvider.GetOutputPorts().Count(p => p.IsDynamic);

                if (dynamicCount > 1)//2개이상일때만 제거버튼 활성화
                {
                    var wrapper = new VisualElement {
                        style = {
                            flexDirection = FlexDirection.Row,
                            alignItems = Align.Center
                        }
                    };

                    var removeButton = new Button(() =>
                    {
                        m_nodeContext.RequestRemovePort(port);
                    })
                    {
                        text = "X",
                        tooltip = "현재 포트를 제거합니다",
                        style =
                        {
                            marginLeft = 4,
                            minWidth = 18,
                            maxWidth = 18,
                            height = 16,
                            alignSelf = Align.Center
                        }
                    };

                    //방향별 UI 구성
                    if (direction == Direction.Input)
                    {
                        wrapper.Add(port);
                        wrapper.Add(removeButton);
                    }
                    else
                    {
                        wrapper.Add(removeButton);
                        wrapper.Add(port);
                    }

                    portElement = wrapper;
                }
            }

            container.Add(portElement);
            m_listPorts.Add(port);
        }
        
        private void CreateAddDynamicPortButton(VisualElement container, Direction direction)
        {
            var row = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    justifyContent = direction == Direction.Output ? Justify.FlexEnd : Justify.FlexStart,
                    alignItems = Align.Center,
                    marginTop = 6,
                    marginBottom = 4
                }
            };

            var addButton = new Button(() =>
            {
                m_nodeContext.RequestAddDynamicPort(direction, m_graphNode);
            })
            {
                text = direction == Direction.Output ? "+ Add Output" : "+ Add Input",
                tooltip = direction == Direction.Output ? "출력 포트를 추가합니다" : "입력 포트를 추가합니다",
                style =
                {
                    paddingLeft = 6,
                    paddingRight = 6,
                    height = 20,
                    marginLeft = direction == Direction.Input ? 10 : 0,
                    marginRight = direction == Direction.Output ? 10 : 0,
                    alignSelf = Align.Center
                }
            };

            row.Add(addButton);
            container.Add(row);
        }

        private string GetMaxConnectionSymbol(int maxConnection)
        {
            return maxConnection < 0 ? DataGraphSettings.c_Infinity : maxConnection.ToString();
        }
		
		private void CreateNodeInfoNameDescription()
		{
			//NodeInfo 기본 정보 및 구분 선
			var separator = new VisualElement
			{
				name = "port-label-separator",
				style =
				{
					height = 1,
					backgroundColor = new StyleColor(new Color(.137f, 0.137f, .137f)),
				}
			};
			var borderElement = this.Q<VisualElement>("node-border");
			if (borderElement != null)
			{
				borderElement.Add(separator);
				borderElement.Add(CreateDescriptionLabel());
			}
			else
			{
				Debug.LogWarning("node-border not found!");
			}
		}

		private VisualElement CreateDescriptionLabel()
		{
			if(m_descriptionContainer == null)
			{
				//TODO Settings SO로 세팅 값 빼주기 or AddToClassList로 uss에서 세팅 가능하도록 하기
				m_descriptionContainer = new VisualElement();
				m_descriptionContainer.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
				m_descriptionContainer.style.color = new StyleColor(new Color(0.82f, 0.82f, 0.82f));
				m_descriptionContainer.style.paddingLeft = 8;
				m_descriptionContainer.style.paddingRight = 8;
				m_descriptionContainer.style.paddingTop = 8;
				m_descriptionContainer.style.paddingBottom = 8;
                
                //최대 가로 길이 ( 줄바꿈 용임
                m_descriptionContainer.style.maxWidth = 320;
			}
			else
			{
				m_descriptionContainer.Clear();
			}
			m_descriptionContainer.style.flexDirection = FlexDirection.Column;
			
			var nameLabel = new Label($"{m_nodeInfoName}")
			{
				style = { 
					unityFontStyleAndWeight = FontStyle.Bold ,
					whiteSpace = WhiteSpace.Normal,
				}
			};

			var descLabel = new Label($"{m_nodeInfoDescription}")
			{
				style = { 
					unityTextAlign = TextAnchor.UpperLeft ,
					whiteSpace = WhiteSpace.Normal,
				}
			};

			m_descriptionContainer.Add(nameLabel);
			m_descriptionContainer.Add(descLabel);

			return m_descriptionContainer;
		}

		public void UpdateDescription(string newName, string newDescription)
		{
			m_nodeInfoName = newName;
			m_nodeInfoDescription = newDescription;

			CreateDescriptionLabel();
		}
        
        private void CreateSubGraphObjectField(
            string label,
            DataGraph currentValue,
            System.Type graphType,
            System.Action<DataGraph> onValueChanged)
        {
            var subGraphField = new ObjectField(label)
            {
                objectType = graphType,
                allowSceneObjects = false,
                value = currentValue
            };

            subGraphField.style.flexGrow = 0;
            subGraphField.style.width = 280;

            subGraphField.RegisterValueChangedCallback(evt =>
            {
                if (Equals(currentValue, evt.newValue))
                    return;

                onValueChanged?.Invoke(evt.newValue as DataGraph);

                /*if (EditorWindow.HasOpenInstances<DataGraphEditorWindow>())
                {
                    var editorWindow = EditorWindow.GetWindow<DataGraphEditorWindow>();
                    editorWindow?.SetAssetDirty();
                }*/
                m_nodeContext?.SetAssetDirty();
                
                m_graphNode.NotifyNodeValueChangedEvent();
                currentValue = evt.newValue as DataGraph;
            });

            mainContainer.Add(subGraphField);
        }

		private void AddInlineFieldsFromAttributes()
		{
			//TODO Settings SO로 세팅 값 빼주기
			var inlineContainer = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Column,
					marginTop = 4,
					marginBottom = 4,
					paddingLeft = 4,
					paddingRight = 4
				}
			};

			var targetType = m_graphNode.GetType();
			var nodeBodyMembers = targetType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(m => m.GetCustomAttribute<NodeBodyAttribute>() != null);

			foreach (var member in nodeBodyMembers) //NodeBody가 붙어있는 멤버에게만 적용한다.
			{
				var attr = member.GetCustomAttribute<NodeBodyAttribute>();
                var tooltipAttr = member.GetCustomAttribute<NodeBodyTooltipAttribute>();
                
                string label = attr.Label ?? member.Name;
                string tooltip = tooltipAttr?.Content;

				Type memberType;
				object value;
				Action<object> setter;

				if (member is FieldInfo field)
				{
					memberType = field.FieldType;
					value = field.GetValue(m_graphNode);
                    setter = val =>
                    {
                        field.SetValue(m_graphNode, val);
                        //TODO : 개선..
                        /*if (EditorWindow.HasOpenInstances<DataGraphEditorWindow>())
                        {
                            var editorWindow = EditorWindow.GetWindow<DataGraphEditorWindow>();
                            editorWindow?.SetAssetDirty();
                        }*/
                        m_nodeContext?.SetAssetDirty();
                    };
				}
				else if (member is PropertyInfo prop && prop.CanWrite && prop.CanRead)
				{
					memberType = prop.PropertyType;
					value = prop.GetValue(m_graphNode);
                    setter = val =>
                    {
                        prop.SetValue(m_graphNode, val);
                        //TODO : 개선..
                        /*if (EditorWindow.HasOpenInstances<DataGraphEditorWindow>())
                        {
                            var editorWindow = EditorWindow.GetWindow<DataGraphEditorWindow>();
                            editorWindow?.SetAssetDirty();
                        }*/
                        m_nodeContext?.SetAssetDirty();
                    };
				}
				else continue;

				VisualElement fieldElement = CreateInputField(memberType, value, setter);
				if (fieldElement != null)
				{
					var container = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
                    var labelElement = new Label(label) { style = { minWidth = 50, unityTextAlign = TextAnchor.MiddleLeft } };
                    if (!string.IsNullOrEmpty(tooltip))
                    {
                        labelElement.tooltip = tooltip;
                        fieldElement.tooltip = tooltip; //필드에도 같이 적용
                    }
                    
                    container.Add(labelElement);
                    container.Add(fieldElement);
                    inlineContainer.Add(container);
				}
			}
			mainContainer.Add(inlineContainer);
		}

        private VisualElement CreateInputField(Type type, object value, Action<object> onChanged)
        {
            if (type.IsEnum && value is Enum enumVal)
            {
                bool isFlags = Attribute.IsDefined(type, typeof(FlagsAttribute));
                if (isFlags)
                {
                    var flagsField = new EnumFlagsField(enumVal) {
                        style = {
                            minWidth = 140,
                            flexGrow = 1
                        }
                    };
                    flagsField.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
                    return flagsField;
                }
                else
                {
                    var enumField = new EnumField(enumVal);
                    enumField.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
                    return enumField;
                }
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int32:
                        var intField = new IntegerField { value = (int)value };
                        intField.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
                        return intField;

                    case TypeCode.Single: // float
                        var floatField = new FloatField { value = (float)value };
                        floatField.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
                        return floatField;

                    case TypeCode.String:
                        var textField = new TextField
                        {
                            value = (string)value,
                            style = {
                                minWidth = 100,
                                flexGrow = 1
                            }
                        };
                        textField.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
                        return textField;

                    case TypeCode.Boolean:
                        var toggle = new Toggle { value = (bool)value };
                        toggle.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
                        return toggle;
                }
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))//ScriptableObject, Texture, GameObject 등.. JSON 익스포트는 안될테니 유의!!!!!!!!!!!!!!!!!!!!
            {
                var objectField = new ObjectField
                {
                    objectType = type,
                    allowSceneObjects = false,
                    value = (UnityEngine.Object)value
                };
                objectField.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
                return objectField;
            }

            return new Label($"Unsupported: {type?.Name ?? "null"}");
        }
        
        //----------------레거시 이넘 필드... Mixed라고 뜨는거 싫어서 옆에 다 표시하게 하려다가 그냥 말았음
        private VisualElement CreateFlagsField(Enum current, Action<object> onChanged)
        {
            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                }
            };
            var flagsField = new EnumFlagsField(current) {
                style = {
                    minWidth = 140,
                    flexGrow = 1
                }
            };
            var pretty = new Label(FormatSelectedFlags(current)) {
                style = {
                    whiteSpace = WhiteSpace.Normal
                }
            };
            flagsField.RegisterValueChangedCallback(evt =>
            {
                onChanged(evt.newValue);
                pretty.text = FormatSelectedFlags((Enum)evt.newValue);
            });

            row.Add(flagsField);
            row.Add(pretty);
            return row;
        }

        private static string FormatSelectedFlags(Enum value)
        {
            var parts = new List<string>();
            long cur = Convert.ToInt64(value);
            foreach (Enum flag in Enum.GetValues(value.GetType()))
            {
                long f = Convert.ToInt64(flag);
                if (f == 0) continue;                 // None 생략
                if ((cur & f) == f) parts.Add(flag.ToString());
            }
            return parts.Count == 0 ? "None" : string.Join(", ", parts);
        }
    }
}