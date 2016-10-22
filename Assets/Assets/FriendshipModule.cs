using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Friendship
/// Created by Timwi
/// </summary>
public class FriendshipModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMSelectable Selectable;
    public KMAudio Audio;

    public GameObject FsScreen;
    public Mesh PlaneMesh;

    static string[] _ponyNames = new[] {
            "Aloe Blossom", "Amethyst Star", "Apple Cinnamon", "Apple Fritter", "Babs Seed", "Berry Punch", "Big McIntosh",
            "Bulk Biceps", "Cadance", "Carrot Top", "Celestia", "Cheerilee", "Cheese Sandwich", "Cherry Jubilee",
            "Coco Pommel", "Coloratura", "Daisy", "Daring Do", "Derpy Hooves", "Diamond Tiara", "Double Diamond",
            "Filthy Rich", "Granny Smith", "Hoity Toity", "Lightning Dust", "Lily", "Lotus Blossom", "Luna",
            "Lyra Heartstrings", "Maud Pie", "Mayor Mare", "Moon Dancer", "Night Light", "Nurse Redheart", "Octavia Melody",
            "Roseluck", "Screwball", "Shining Armor", "Silver Shill", "Silver Spoon", "Silverstar", "Spoiled Rich",
            "Starlight Glimmer", "Sunburst", "Sunset Shimmer", "Suri Polomare", "Thunderlane", "Time Turner", "Toe Tapper",
            "Tree Hugger", "Trenderhoof", "Trixie", "Trouble Shoes", "Twilight Velvet", "Twist", "Vinyl Scratch" };

    static string[] _elementsOfHarmony = new[] {
            "Altruism", "Amicability", "Benevolence", "Caring", "Charitableness", "Compassion", "Conscientiousness",
            "Consideration", "Courage", "Fairness", "Flexibility", "Generosity", "Helpfulness", "Honesty",
            "Inspiration", "Kindness", "Laughter", "Love", "Loyalty", "Open-mindedness", "Patience",
            "Resoluteness", "Selflessness", "Sincerity", "Solidarity", "Support", "Sympathy", "Thoughtfulness" };

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

        public int RowOrCol
        {
            get
            {
                if (Symbol > 28)
                    return 13 - (Symbol % 14);
                return Symbol % 14;
            }
        }

        public override string ToString()
        {
            return string.Format("(X={0}, Y={1}, Pony={2} ({3}))", X, Y, _ponyNames[Symbol], IsRowSymbol ? "row" : "col");
        }
    }

    void Start()
    {
        Module.OnActivate += ActivateModule;

        tryAgain:

        // 16 × 11
        var allowed = @"
###########XXXXX
############XXXX
############XXXX
#############XXX
###############X
################
X###############
XXX#############
XXXX############
XXXX############
XXXXX###########".Replace("\r", "").Substring(1).Split('\n').Select(row => row.Reverse().Select(ch => ch == '#').ToArray()).ToArray();

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
        Debug.Log("Friendship symbols:\n" + string.Join("\n", friendshipSymbols.Select(s => s.ToString()).ToArray()));

        // Which column and row symbols should the expert disregard?
        var disregardCol = friendshipSymbols.Where(s => !s.IsRowSymbol && !friendshipSymbols.Any(s2 => s2 != s && s2.X == s.X)).OrderBy(s => s.X).FirstOrDefault();
        if (disregardCol == null)
            goto tryAgain;
        Debug.LogFormat("Disregard column symbol: {0}", _ponyNames[disregardCol.Symbol]);

        var disregardRow = friendshipSymbols.Where(s => s.IsRowSymbol && !friendshipSymbols.Any(s2 => s2 != s && s2.Y == s.Y)).OrderByDescending(s => s.Y).FirstOrDefault();
        if (disregardRow == null)
            goto tryAgain;
        Debug.LogFormat("Disregard row symbol: {0}", _ponyNames[disregardRow.Symbol]);

        // Which Elements of Harmony are at the intersections of the remaining columns and rows?
        var deducedElementsOfHarmony =
            friendshipSymbols.Where(s => !s.IsRowSymbol && s != disregardCol).SelectMany(cs =>
            friendshipSymbols.Where(s => s.IsRowSymbol && s != disregardRow).Select(rs => _grid[rs.RowOrCol][cs.RowOrCol])).ToArray();

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
        var correctElementOfHarmony = deducedElementsOfHarmony[Rnd.Range(0, 4)];
        displayedElementsOfHarmony.Insert(Rnd.Range(0, displayedElementsOfHarmony.Count + 1), correctElementOfHarmony);

        Debug.LogFormat("Showing Elements of Harmony:\n{0}\n(of which {1} is correct)", string.Join("\n", displayedElementsOfHarmony.Select(d => _elementsOfHarmony[d]).ToArray()), _elementsOfHarmony[correctElementOfHarmony]);

        // Create the GameObjects to display the friendship symbols on the module.
        foreach (var friendshipSymbol in friendshipSymbols)
        {
            var graphic = new GameObject();
            graphic.transform.parent = FsScreen.transform;
            graphic.transform.localPosition = new Vector3(friendshipSymbol.X * .029f / 3 - .072f, 0.0001f, friendshipSymbol.Y * .029f / 3 - .024f);
            graphic.transform.localRotation = new Quaternion(0, 180, 0, 1);
            graphic.transform.localScale = new Vector3(.0029f, .0029f, .0029f);
            graphic.AddComponent<MeshFilter>().mesh = PlaneMesh;
            var mr = graphic.AddComponent<MeshRenderer>();
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(string.Format(@"D:\c\KTANE\Friendship\Manual\img\Friendship Symbol {0:00}.png", friendshipSymbol.Symbol)));
            mr.material.mainTexture = tex;
            mr.material.shader = Shader.Find("Unlit/Transparent");
        }
    }

    void ActivateModule()
    {
    }
}
