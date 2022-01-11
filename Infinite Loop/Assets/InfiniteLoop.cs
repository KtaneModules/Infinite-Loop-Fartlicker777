using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class InfiniteLoop : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMSelectable[] Arrows;
   public GameObject MorseLight;
   public GameObject StatusLight;
   public Material[] MorseColors;
   public TextMesh[] Letters;
   public GameObject[] Boxes;
   public Material[] RightWrongColors;

   int[] LetterIndexes = { 0, 0, 0, 0, 0, 0};

   readonly string[] MorseLetters = { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
   readonly string[] WordList = { "anchor", "axions", "brutal", "bunker", "ceased", "cypher", "demote", "devoid", "ejects", "expend", "fixate", "fondly", "geyser", "guitar", "hexing", "hybrid", "incite", "inject", "jacked", "jigsaw", "kayaks", "komodo", "lazuli", "logjam", "maimed", "musket", "nebula", "nuking", "overdo", /*"oxides"*/"oblong", "photon", "probed", "quartz", "quebec", "refute", "regime", "sierra", "swerve", "tenacy", "thymes", "ultima", "utopia", "valved", "viable", "wither", "wrench", "xenons", "xylose", "yanked", "yellow", "zigged", "zodiac" };
   readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz";
   string MorseVersion = "";
   string SelectedWord = "";

   Coroutine Check;
   Coroutine Flashing;
   Coroutine Solve;

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   void Awake () {
      moduleId = moduleIdCounter++;

      foreach (KMSelectable Arrow in Arrows) {
         Arrow.OnInteract += delegate () { ArrowPress(Arrow); return false; };
      }

   }

   void Start () {
      SelectedWord = WordList[Random.Range(0, WordList.Length)];
      StatusLight.gameObject.SetActive(false);
      for (int i = 0; i < 6; i++) {
         for (int j = 0; j < 26; j++) {
            if (SelectedWord[i] == Alphabet[j]) {
               MorseVersion += MorseLetters[j];
            }
         }
      }
      Debug.LogFormat("[Infinite Loop #{0}] The selected word is {1}. The morse equivalent is \"{2}\"", moduleId, SelectedWord, MorseVersion);
      Debug.LogFormat("[Infinite Loop #{0}] The extended version is \"{1}{1}{1}\"", moduleId, MorseVersion);
      for (int i = 0; i < 6; i++) {
         Letters[i].text = "A";
      }
      Flashing = StartCoroutine(Flash());
   }

   void ArrowPress (KMSelectable Arrow) {
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Arrow.transform);
      if (moduleSolved || Solve != null) {
         return;
      }
      for (int i = 0; i < 12; i++) {
         if (Arrow == Arrows[i] && i < 6) {
            LetterIndexes[i % 6] = (LetterIndexes[i % 6] + 1) % 26;
            Letters[i % 6].text = Alphabet[LetterIndexes[i % 6]].ToString().ToUpper();
         }
         else if (Arrow == Arrows[i]) {
            LetterIndexes[i % 6]--;
            if (LetterIndexes[i % 6] < 0) {
               LetterIndexes[i % 6] += 26;
            }
            Letters[i % 6].text = Alphabet[LetterIndexes[i % 6]].ToString().ToUpper();
         }
      }
      if (Check != null) {
         StopCoroutine(Check);
      }
      Check = StartCoroutine(Checking());
   }

   IEnumerator Checking () {
      yield return new WaitForSecondsRealtime(3f);
      if (Letters[0].text + Letters[1].text + Letters[2].text + Letters[3].text + Letters[4].text + Letters[5].text == SelectedWord.ToUpper()) {
         StopCoroutine(Flashing);
         MorseLight.GetComponent<MeshRenderer>().material = MorseColors[0];
         Solve = StartCoroutine(Solving());
         while (Solve != null) {
            yield return null;
         }
         Audio.PlaySoundAtTransform("victory", transform);
         GetComponent<KMBombModule>().HandlePass();
         moduleSolved = true;
         MorseLight.GetComponent<MeshRenderer>().material = RightWrongColors[0];
      }
      else {
         Solve = StartCoroutine(Solving());
         StopCoroutine(Flashing);
         MorseLight.GetComponent<MeshRenderer>().material = MorseColors[0];
         while (Solve != null) {
            yield return null;
         }
         GetComponent<KMBombModule>().HandleStrike();
         MorseLight.GetComponent<MeshRenderer>().material = RightWrongColors[1];
         yield return new WaitForSeconds(1f);
         Flashing = StartCoroutine(Flash());
         for (int i = 0; i < 6; i++) {
            Boxes[i].GetComponent<MeshRenderer>().material = MorseColors[0];
            LetterIndexes[i] = 0;
            Letters[i % 6].text = "A";
            yield return new WaitForSecondsRealtime(0.2f);
         }
      }
   }

   IEnumerator Solving () {
      for (int i = 0; i < 6; i++) {
         if (Letters[i].text.ToString().ToUpper() == SelectedWord[i].ToString().ToUpper()) {
            Boxes[i].GetComponent<MeshRenderer>().material = RightWrongColors[0];
         }
         else {
            Boxes[i].GetComponent<MeshRenderer>().material = RightWrongColors[1];
         }
         Letters[i].text = "";
         Audio.PlaySoundAtTransform("sound" + (i + 1).ToString(), transform);
         yield return new WaitForSecondsRealtime(0.2f);
      }
      Solve = null;
   }

   IEnumerator Flash () {
      int Index = Random.Range(0, MorseVersion.Length);
      while (true) {
         if (MorseVersion[Index % MorseVersion.Length] == '.') {
            MorseLight.GetComponent<MeshRenderer>().material = MorseColors[1];
            yield return new WaitForSecondsRealtime(.3f);
         }
         else {
            MorseLight.GetComponent<MeshRenderer>().material = MorseColors[1];
            yield return new WaitForSecondsRealtime(.9f);
         }
         MorseLight.GetComponent<MeshRenderer>().material = MorseColors[0];
         yield return new WaitForSecondsRealtime(.3f);
         Index++;
      }
   }

   void CheckForDupes () {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
      string Test1 = "";
#pragma warning restore IDE0059 // Unnecessary assignment of a value
      string Test2 = "";
      for (int i = 0; i < WordList.Length - 1; i++) {

         Test1 = "";


         for (int x = 0; x < 6; x++) {
            for (int y = 0; y < 26; y++) {
               if (WordList[i][x] == Alphabet[y]) {
                  Test1 += MorseLetters[y];
               }
            }
         }

         for (int j = i + 1; j < WordList.Length; j++) {
            for (int x = 0; x < 6; x++) {
               for (int y = 0; y < 26; y++) {
                  if (WordList[j][x] == Alphabet[y]) {
                     Test2 += MorseLetters[y];
                  }
               }
            }
            string Original = Test2;

            for (int p = i + 1; p < Original.Length; p++) {
               Test2 = Test2[Test2.Length - 1].ToString() + Test2.Remove(Test2.Length - 1);
               if (Test2 == Test1) {
                  Debug.LogFormat("{0} matches {1}", WordList[i], WordList[j]);
               }
            }
            Test2 = "";
         }

      }
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} XXXXXX to submit that word.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.Trim();
      yield return null;
      bool result = Command.Any(x => !char.IsLetter(x));
      if (Command.Length != 6 || result) {
         yield return "sendtochaterror I don't understand!";
         yield break;
      }
      for (int i = 0; i < 6; i++) {
         if ("ABCDEFGHIJKLM".Contains(Command[i].ToString().ToUpper())) {
            while (Letters[i].text != Command[i].ToString().ToUpper()) {
               Arrows[i].OnInteract();
               yield return new WaitForSecondsRealtime(.1f);
            }
         }
         else {
            while (Letters[i].text != Command[i].ToString().ToUpper()) {
               Arrows[i + 6].OnInteract();
               yield return new WaitForSecondsRealtime(.1f);
            }
         }
      }
      if (Command.ToLower() == SelectedWord) {
         yield return "solve";
      }
      else {
         yield return "strike";
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return ProcessTwitchCommand(SelectedWord);
      while (!moduleSolved) yield return true;
   }
}
