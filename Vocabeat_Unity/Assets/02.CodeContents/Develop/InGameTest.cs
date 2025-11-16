using LUIZ.UI;
using UnityEngine;

public class InGameTest : MonoBehaviour
{
    [SerializeField] private ManagerUISO UIManager;

    [SerializeField] private UIContainerBase UIContainer;

    [Header("Test")]
    [SerializeField] private SongData_SO TestSongData;
    [SerializeField] private EDifficulty Diff;

    private void Start()
    {
        UIContainer.DoRegisterContainer();

        UIManager.UIShow<UIFrameInGame>().BindSongData(TestSongData, Diff);
    }
}
