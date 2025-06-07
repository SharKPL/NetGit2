using UnityEngine;
using TMPro;

namespace MUSOAR
{
    public class SaveSlotInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text saveNameText;
        [SerializeField] private TMP_Text saveDateText;
        [SerializeField] private TMP_Text saveTimeText;

        public TMP_Text SaveNameText => saveNameText;
        public TMP_Text SaveDateText => saveDateText;
        public TMP_Text SaveTimeText => saveTimeText;
    }
}
