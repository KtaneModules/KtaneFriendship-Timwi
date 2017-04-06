using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Friendship
/// Created by Timwi
/// </summary>
public class FriendshipModule : MonoBehaviour
{
    public KMBombModule Module;
    public KMAudio Audio;

    public GameObject FsScreen;
    public GameObject FsCylinder;
    public Mesh PlaneMesh;

    public TextMesh[] ElementsOfHarmony;
    public KMSelectable BtnUp;
    public KMSelectable BtnDown;
    public KMSelectable BtnSubmit;

    private int _correctElementOfHarmony;
    private int _selectedElementOfHarmony = 0;
    private bool _isCoroutineRunning = false;
    private bool _isCylinderMovingUp;
    private int[] _displayedElementsOfHarmony;
    private Quaternion[] _cylinderRotations;
    private int _rotationAnimationSteps;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    static string[] _ponyNames = new[] {
        "Amethyst Star", "Apple Cinnamon", "Apple Fritter", "Babs Seed", "Berry Punch", "Big McIntosh", "Bulk Biceps",
        "Cadance", "Carrot Top", "Celestia", "Cheerilee", "Cheese Sandwich", "Cherry Jubilee", "Coco Pommel",
        "Coloratura", "Daisy", "Daring Do", "Derpy", "Diamond Tiara", "Double Diamond", "Filthy Rich",
        "Granny Smith", "Hoity Toity", "Lightning Dust", "Lily", "Luna", "Lyra Heartstrings", "Maud Pie",
        "Mayor Mare", "Moon Dancer", "Ms. Harshwhinny", "Night Light", "Nurse Redheart", "Octavia Melody", "Rose",
        "Screwball", "Shining Armor", "Silver Shill", "Silver Spoon", "Silverstar", "Spoiled Rich", "Starlight Glimmer",
        "Sunburst", "Sunset Shimmer", "Suri Polomare", "Sweetie Drops", "Thunderlane", "Time Turner", "Toe Tapper",
        "Tree Hugger", "Trenderhoof", "Trixie", "Trouble Shoes", "Twilight Velvet", "Twist", "Vinyl Scratch" };

    static string[] _elementsOfHarmony = new[] {
            "Altruism", "Amicability", "Authenticity", "Benevolence", "Caring", "Charitableness", "Compassion",
            "Conscientiousness", "Consideration", "Courage", "Fairness", "Flexibility", "Generosity", "Helpfulness", "Honesty",
            "Inspiration", "Kindness", "Laughter", "Loyalty", "Open-mindedness", "Patience",
            "Resoluteness", "Selflessness", "Sincerity", "Solidarity", "Support", "Sympathy", "Thoughtfulness" };

    static float[] _elementsOfHarmonyScaleX = new[] {
        .0050f, .0050f, .0050f, .0050f, .0050f, .0044f, .0050f,
        .0038f, .0048f, .0050f, .0050f, .0050f, .0050f, .0050f,
        .0050f, .0050f, .0050f, .0050f, .0050f, .0038f, .0050f,
        .0050f, .0050f, .0050f, .0050f, .0050f, .0050f, .0042f };

    static int[][] _grid = new[] {
            new[] { 8, 6, 18, 9, 19, 26, 10, 2, 7, 24, 20, 13, 11, 15 },
            new[] { 25, 16, 26, 18, 12, 8, 27, 22, 5, 13, 14, 2, 15, 24 },
            new[] { 14, 15, 7, 24, 5, 25, 8, 4, 26, 17, 12, 27, 0, 21 },
            new[] { 3, 23, 16, 7, 18, 4, 17, 13, 19, 8, 10, 0, 24, 25 },
            new[] { 0, 5, 23, 17, 11, 13, 15, 20, 16, 21, 18, 12, 6, 1 },
            new[] { 19, 9, 6, 13, 14, 3, 18, 10, 23, 7, 11, 15, 4, 2 },
            new[] { 24, 27, 17, 5, 1, 21, 3, 18, 22, 23, 15, 10, 7, 11 },
            new[] { 6, 24, 27, 8, 26, 23, 21, 9, 0, 22, 16, 20, 25, 3 },
            new[] { 9, 17, 5, 1, 8, 14, 23, 16, 4, 2, 13, 18, 20, 10 },
            new[] { 16, 11, 0, 2, 25, 7, 4, 1, 6, 5, 19, 21, 10, 12 },
            new[] { 26, 25, 19, 10, 27, 15, 9, 3, 17, 14, 1, 22, 21, 0 },
            new[] { 20, 26, 24, 14, 6, 22, 19, 17, 25, 12, 23, 1, 2, 13 },
            new[] { 11, 0, 20, 27, 7, 9, 22, 8, 12, 3, 21, 4, 26, 5 },
            new[] { 22, 12, 1, 6, 20, 16, 11, 14, 9, 27, 2, 19, 3, 4 }
        };

    class SymbolInfo
    {
        public int X;
        public int Y;
        public int Symbol;
        public bool IsRowSymbol;
        public int RowOrCol { get { return Symbol >= 28 ? 13 - (Symbol % 14) : Symbol % 14; } }
        public override string ToString() { return string.Format("(X={0}, Y={1}, Pony={2} ({3}))", X, Y, _ponyNames[Symbol], IsRowSymbol ? "row" : "col"); }
    }

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        tryAgain:
        var logging = new StringBuilder();

        // 13 × 9
        var allowed = @"
#########XXXX
#########XXXX
##########XXX
###########XX
#############
XX###########
XXX##########
XXXX#########
XXXX#########".Replace("\r", "").Substring(1).Split('\n').Select(row => row.Reverse().Select(ch => ch == '#').ToArray()).ToArray();

        var friendshipSymbols = new List<SymbolInfo>();
        var available = Enumerable.Range(0, 56).ToList();
        var rowSymbols = 0;
        var colSymbols = 0;

        for (var cix = 0; cix < 6; cix++)
        {
            // Choose a coordinate to place the next friendship symbol.
            var coords = allowed.SelectMany((row, yy) => row.Select((b, xx) => b ? new { X = xx, Y = yy } : null)).Where(inf => inf != null).ToList();
            if (coords.Count == 0)
                goto tryAgain;

            var coord = coords[Rnd.Range(0, coords.Count)];
            var x = coord.X;
            var y = coord.Y;
            allowed[y][x] = false;

            // Make sure that future friendship symbols won’t overlap with this one.
            for (var xx = -2; xx < 3; xx++)
                for (var yy = -2; yy < 3; yy++)
                    if (y + yy >= 0 && x + xx >= 0 && y + yy < allowed.Length && x + xx < allowed[y + yy].Length)
                        allowed[y + yy][x + xx] = false;

            // Choose a friendship symbol.
            var fsIx = Rnd.Range(0, available.Count);
            var fs = available[fsIx];
            available.RemoveAt(fsIx);

            // Remove the other friendship symbol from consideration that represents the same row/column as this one
            available.RemoveAll(ix => (ix / 14) % 2 == (fs / 14) % 2 && ix / 14 != fs / 14 && ix % 14 == 13 - (fs % 14));

            // Determine whether this is a row or column symbol.
            var isRowSymbol = (fs / 14) % 2 != 0;
            if (isRowSymbol)
            {
                rowSymbols++;
                if (rowSymbols == 3)
                    // If we now have 3 row symbols, remove all the other row symbols from consideration.
                    available.RemoveAll(ix => (ix / 14) % 2 != 0);
            }
            else
            {
                colSymbols++;
                if (colSymbols == 3)
                    // If we now have 3 column symbols, remove all the other column symbols from consideration.
                    available.RemoveAll(ix => (ix / 14) % 2 == 0);
            }

            friendshipSymbols.Add(new SymbolInfo { X = x, Y = y, IsRowSymbol = isRowSymbol, Symbol = fs });
        }
        for (int i = 0; i < friendshipSymbols.Count; i++)
            logging.AppendLine(string.Format("[Friendship #{{0}}] Friendship symbol #{0}: {1}", i + 1, friendshipSymbols[i]));

        // Which column and row symbols should the expert disregard?
        var disregardCol = friendshipSymbols.Where(s => !s.IsRowSymbol && !friendshipSymbols.Any(s2 => s2 != s && s2.X == s.X)).OrderBy(s => s.X).FirstOrDefault();
        if (disregardCol == null)
            goto tryAgain;
        logging.AppendLine(string.Format("[Friendship #{{0}}] Disregard column symbol {0}, leaving {1}", _ponyNames[disregardCol.Symbol], string.Join(" and ", friendshipSymbols.Where(s => !s.IsRowSymbol && s != disregardCol).Select(s => _ponyNames[s.Symbol]).ToArray())));

        var disregardRow = friendshipSymbols.Where(s => s.IsRowSymbol && !friendshipSymbols.Any(s2 => s2 != s && s2.Y == s.Y)).OrderByDescending(s => s.Y).FirstOrDefault();
        if (disregardRow == null)
            goto tryAgain;

        logging.AppendLine(string.Format("[Friendship #{{0}}] Disregard row symbol {0}, leaving {1}", _ponyNames[disregardRow.Symbol], string.Join(" and ", friendshipSymbols.Where(s => s.IsRowSymbol && s != disregardRow).Select(s => _ponyNames[s.Symbol]).ToArray())));
        Debug.LogFormat(logging.ToString(), _moduleId);

        // Which Elements of Harmony are at the intersections of the remaining columns and rows?
        var deducedElementsOfHarmony =
            friendshipSymbols.Where(s => !s.IsRowSymbol && s != disregardCol).SelectMany(cs =>
            friendshipSymbols.Where(s => s.IsRowSymbol && s != disregardRow).Select(rs => _grid[rs.RowOrCol][cs.RowOrCol])).ToArray();
        Debug.LogFormat("[Friendship #{0}] The potential Elements of Harmony are: {1}", _moduleId, string.Join(", ", deducedElementsOfHarmony.Select(ix => _elementsOfHarmony[ix]).ToArray()));

        // On the bomb, display 6 wrong Elements of Harmony...
        var displayedElementsOfHarmony = new List<int>();
        var availableElementsOfHarmony = Enumerable.Range(0, 28).Except(deducedElementsOfHarmony).ToList();
        for (int i = 0; i < 6; i++)
        {
            var ix = Rnd.Range(0, availableElementsOfHarmony.Count);
            displayedElementsOfHarmony.Add(availableElementsOfHarmony[ix]);
            availableElementsOfHarmony.RemoveAt(ix);
        }
        // ... plus one of the correct ones in a random place
        _correctElementOfHarmony = deducedElementsOfHarmony[Rnd.Range(0, 4)];
        displayedElementsOfHarmony.Insert(Rnd.Range(0, displayedElementsOfHarmony.Count + 1), _correctElementOfHarmony);
        _displayedElementsOfHarmony = displayedElementsOfHarmony.ToArray();

        Debug.LogFormat("[Friendship #{1}] Showing Elements of Harmony: {0}", string.Join(", ", _displayedElementsOfHarmony.Select(d => _elementsOfHarmony[d]).ToArray()), _moduleId);
        Debug.LogFormat("[Friendship #{1}] Correct Element of Harmony: {0}", _elementsOfHarmony[_correctElementOfHarmony], _moduleId);

        for (int i = 0; i < 7; i++)
        {
            ElementsOfHarmony[i].text = _elementsOfHarmony[_displayedElementsOfHarmony[i]];
            ElementsOfHarmony[i].transform.localScale = new Vector3(_elementsOfHarmonyScaleX[_displayedElementsOfHarmony[i]], .005f, .005f);
        }

        // Create the GameObjects to display the friendship symbols on the module.
        foreach (var friendshipSymbol in friendshipSymbols)
        {
            var graphic = new GameObject { name = _ponyNames[friendshipSymbol.Symbol] };
            graphic.transform.parent = FsScreen.transform;
            graphic.transform.localPosition = new Vector3(friendshipSymbol.X * .035f / 3 - .07f, 0.0001f, friendshipSymbol.Y * .035f / 3 - .022f);
            graphic.transform.localRotation = new Quaternion(0, 180, 0, 1);
            graphic.transform.localScale = new Vector3(.0035f, .0035f, .0035f);
            graphic.AddComponent<MeshFilter>().mesh = PlaneMesh;
            var mr = graphic.AddComponent<MeshRenderer>();
            var tex = new Texture2D(2, 2);
            tex.LoadImage(Friendship.FriendshipSymbols.RawBytes[friendshipSymbol.Symbol]);
            mr.material.mainTexture = tex;
            mr.material.shader = Shader.Find("Unlit/Transparent");
        }

        BtnSubmit.OnInteract += delegate { handleSubmit(); return false; };
        BtnUp.OnInteract += delegate { go(up: true); return false; };
        BtnDown.OnInteract += delegate { go(up: false); return false; };

        _cylinderRotations = new Quaternion[7];
        for (int i = 0; i < 7; i++)
        {
            _cylinderRotations[i] = FsCylinder.transform.localRotation;
            FsCylinder.transform.Rotate(new Vector3(360f / 7, 0, 0));
        }
        FsCylinder.transform.localRotation = _cylinderRotations[0];
    }

    private void go(bool up)
    {
        (up ? BtnUp : BtnDown).AddInteractionPunch(.25f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, (up ? BtnUp : BtnDown).transform);

        _selectedElementOfHarmony = (_selectedElementOfHarmony + (up ? 6 : 1)) % 7;
        _rotationAnimationSteps = 30;

        if (!_isCoroutineRunning)
        {
            _isCoroutineRunning = true;
            StartCoroutine(cylinderRotation());
        }
    }

    private IEnumerator cylinderRotation()
    {
        while (true)
        {
            if (--_rotationAnimationSteps == 0)
            {
                FsCylinder.transform.localRotation = _cylinderRotations[_selectedElementOfHarmony];
                _isCoroutineRunning = false;
                yield break;
            }

            FsCylinder.transform.localRotation = Quaternion.Slerp(FsCylinder.transform.localRotation, _cylinderRotations[_selectedElementOfHarmony], .25f);
            yield return null;
        }
    }

    private void handleSubmit()
    {
        BtnSubmit.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BtnSubmit.transform);

        Debug.LogFormat("[Friendship #{2}] You selected {0}; correct is {1}.", _elementsOfHarmony[_displayedElementsOfHarmony[_selectedElementOfHarmony]], _elementsOfHarmony[_correctElementOfHarmony], _moduleId);
        if (_displayedElementsOfHarmony[_selectedElementOfHarmony] == _correctElementOfHarmony)
        {
            Module.HandlePass();
            Audio.PlaySoundAtTransform("Yay", Module.transform);
        }
        else
            Module.HandleStrike();
    }

    IEnumerator ProcessTwitchCommand(string command)
    {
        switch (command)
        {
            case "cycle":
                for (int i = 0; i < 7; i++)
                {
                    if (i > 0)
                        yield return new WaitForSeconds(1f);
                    yield return BtnDown;
                    yield return new WaitForSeconds(.1f);
                    yield return BtnDown;
                }
                break;

            default:
                var index = -1;
                for (int i = 0; i < _elementsOfHarmony.Length; i++)
                    if (_elementsOfHarmony[i].Equals(command, StringComparison.OrdinalIgnoreCase))
                        index = i;
                if (index == -1)
                    yield break;
                for (int i = 0; i < 7; i++)
                {
                    if (i > 0)
                    {
                        yield return BtnDown;
                        yield return new WaitForSeconds(.1f);
                        yield return BtnDown;
                    }

                    if (_displayedElementsOfHarmony[_selectedElementOfHarmony] == index)
                    {
                        yield return BtnSubmit;
                        yield return new WaitForSeconds(.1f);
                        yield return BtnSubmit;
                        yield break;
                    }
                }
                break;
        }
    }
}
