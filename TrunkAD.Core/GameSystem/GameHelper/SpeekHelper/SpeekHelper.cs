using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
namespace TrunkAD.Core.GameSystem.GameHelper 
{
    public class SpeekHelper
    {
        public static SpeekHelper Instance { get; set; }

        public void Awake()
        {
            Instance = this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="saying"></param>
        public void Speaking(string saying)
        {
            string say = saying;
            Task task = new Task(() =>
            {
                SpeechSynthesizer speech = new SpeechSynthesizer();
                speech.Volume = 100; //音量
                CultureInfo keyboardCulture = System.Windows.Forms.InputLanguage.CurrentInputLanguage.Culture;
                InstalledVoice neededVoice = speech.GetInstalledVoices(keyboardCulture).FirstOrDefault();
                if (neededVoice == null)
                {
                    say = "未知的操作";
                }
                else
                {
                    speech.SelectVoice(neededVoice.VoiceInfo.Name);
                }
                speech.Speak(say);
            });
            task.Start();
        }
        
    }
}