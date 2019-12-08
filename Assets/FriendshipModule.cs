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
    public KMRuleSeedable RuleSeedable;

    public GameObject FsScreen;
    public GameObject FsCylinder;
    public MeshRenderer FsTemplate;
    public Mesh PlaneMesh;

    public TextMesh[] ElementsOfHarmony;
    public KMSelectable BtnUp;
    public KMSelectable BtnDown;
    public KMSelectable BtnSubmit;
    public Texture[] FriendshipSymbols;

    private int _correctElementOfHarmonyIndex;
    private string[] _displayedElementsOfHarmony;
    private int _selectedElementOfHarmonyIndex = 0;
    private string[] _elementsOfHarmony;    // The elements under the current rule seed; only used by the TP support
    private bool _isCoroutineRunning = false;
    private Quaternion[] _cylinderRotations;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool _isSolved;

    const float _rotationAnimationDuration = .5f;

    static readonly string[] _allPonyNames = new[] {
        "Starstreak", "Coloratura", "Pickpocket", "Lemon Zest", "Night Light", "Bow Hothoof", "Midnight Strike", "Amethyst Star", "Mayor Mare", "Non-pony 6", "Score", "Apple Flora",
        "Sandbar", "Joe", "Spitfire", "Lily", "Snips", "Suri Polomare", "Shoeshine", "Noteworthy", "Rainbow Swoop", "Lucky Clover", "Cherry Berry", "Meadow Song", "Tootsie Flute",
        "Sapphire Shores", "Pursey Pink", "Lemon Hearts", "Filthy Rich", "Snails", "Cherry Jubilee", "Cozy Glow", "Count Caesar", "Mr. Carrot Cake", "Cheerilee", "Cadance",
        "Strawberry Sunrise", "Silverstar", "Lily Lace", "Globe Trotter", "Royal Riff", "Horte Cuisine", "Sealed Scroll", "Granny Smith", "Non-pony 10", "Igneous Rock Pie", "Derpy",
        "Limestone Pie", "Apple Fritter", "Sunset Shimmer", "Daring Do", "Aria Blaze", "Pinny Lane", "Photo Finish", "Ms. Harshwhinny", "Starlight Glimmer", "Stellar Eclipse", "Fleetfoot",
        "Mrs. Cup Cake", "Caramel", "Hoops", "Open Skies", "Moon Dancer", "Minty", "Dr. Caballeron", "Non-pony 5", "Non-pony 9", "Featherweight", "Blossomforth", "Sunshower Raindrops",
        "Spoiled Rich", "Mr. Stripes", "Clear Skies", "Radiant Hope", "Apple Honey", "Sugarcoat", "Shooting Star", "Quibble Pants", "Lyra", "Coco Pommel", "Comet Tail", "Sweetie Belle",
        "Screwball", "Twist", "Starry Eyes", "Indigo Zap", "Trixie", "White Lightning", "Apple Split", "Cloudy Quartz", "Zephyr Breeze", "Leadwing", "Electric Sky", "Silver Shill",
        "Fleur Dis Lee", "Dr. Horse", "Cheese Sandwich", "Flash Sentry", "Apple Bumpkin", "Non-pony 3", "Flax Seed", "Hoity Toity", "Rose", "Scootaloo", "Thunderlane", "Red Gala",
        "Cloud Kicker", "Sour Sweet", "Hayseed Turnip Truck", "Prim Hemline", "Pinkie Pie", "Hondo Flanks", "Twilight Velvet", "Fancy Pants", "Dr. Fauna", "Firecracker Burst",
        "Tender Taps", "Inky Rose", "Screwy", "Golden Gavel", "Blow Dry", "Auntie Applesauce", "Torch Song", "All Aboard", "Emerald Green", "Non-pony 1", "Pear Butter", "Pixel Pizzaz",
        "Biff", "Davenport", "Peachy Sweet", "Peachy Pie", "Strike", "Violet Blurr", "Rarity", "Diamond Tiara", "Trouble Shoes", "Hughbert Jellius", "Babs Seed", "Aloe Blossom", "Flam",
        "Apple Leaves", "Apple Rose", "Lily Blossom", "Plaid Stripes", "Sweetie Drops", "Nurse Redheart", "Non-pony 12", "Sprinkle Medley", "Windy Whistles", "Celestia", "Adagio Dazzle",
        "Saffron Masala", "Non-pony 7", "Truffle", "Feather Bangs", "Twinkleshine", "Vapor Trail", "Non-pony 2", "Golden Harvest", "Octavia Melody", "Apple Bloom", "Unconditioner",
        "Sassaflash", "Magnet Bolt", "Rainbow Dash", "Meadow Flower", "Bright Mac", "Ms. Peachbottom", "Holly Dash", "Silver Spoon", "Rockhoof", "Apple Cobbler", "Mane Moon", "Fluttershy",
        "Apple Strudel", "Diamond Rose", "Luna", "Blue Bobbin", "Fluffy Clouds", "Flitter", "Non-pony 8", "Archer", "Cookie Crumbles", "Lickety Split", "Upper Crust", "Aunt Orange",
        "Withers", "Rogue", "Royal Pin", "Big McIntosh", "Flim", "Double Diamond", "Bumblesweet", "Jet Set", "Toola Roola", "Apple Cinnamon", "Neon Brush", "Clean Sweep", "Coriander Cumin",
        "Sea Swirl", "Compass Star", "Cloudchaser", "Comb Over", "Twilight Sparkle", "Banana Fluff", "Applejack", "Wild Fire", "Cherry Fizzy", "Sunny Daze", "Wheat Grass", "Non-pony 13",
        "Vinyl Scratch", "Maud Pie", "Sunburst", "Prince Blueblood", "Silver Spanner", "Crusoe Palm", "Quick Trim", "Lotus Blossom", "Sunshower", "Orange Swirl", "Daisy", "Sunny Rays",
        "Dumb-Bell", "Goldengrape", "Soarin", "Zipporwhill", "Toe Tapper", "Night Glider", "Star Swirl", "Beauty Brass", "Marble Pie", "Tree Hugger", "Bulk Biceps", "Uncle Orange",
        "Lightning Dust", "Non-pony 4", "Time Turner", "Berryshine", "Gladmane", "Timber Spruce", "Coconut Cream", "Rainbowshine", "Sonata Dusk", "Non-pony 14", "Equality", "Braeburn",
        "Parasol", "Shining Armor", "Trenderhoof" };

    sealed class ElementOfHarmony
    {
        public string Name;
        public double Scale;
        public float ScaleX { get { return (float) (Scale * .0044); } }
    }

    static ElementOfHarmony[] _allElementsOfHarmony = new[] {
        new ElementOfHarmony { Name = "Serenity", Scale = 1 }, new ElementOfHarmony { Name = "Resilience", Scale = 1 }, new ElementOfHarmony { Name = "Scrupulousness", Scale = 1 },
        new ElementOfHarmony { Name = "Adventurousness", Scale = .9 }, new ElementOfHarmony { Name = "Selflessness", Scale = 1 }, new ElementOfHarmony { Name = "Companionship", Scale = 1 },
        new ElementOfHarmony { Name = "Softheartedness", Scale = .9 }, new ElementOfHarmony { Name = "Esteem", Scale = 1 }, new ElementOfHarmony { Name = "Pardoning", Scale = 1 },
        new ElementOfHarmony { Name = "Solidarity", Scale = 1 }, new ElementOfHarmony { Name = "Integrity", Scale = 1 }, new ElementOfHarmony { Name = "Self-respect", Scale = 1 },
        new ElementOfHarmony { Name = "Neutrality", Scale = 1 }, new ElementOfHarmony { Name = "Authenticity", Scale = 1 }, new ElementOfHarmony { Name = "Virtue", Scale = 1 },
        new ElementOfHarmony { Name = "Empathy", Scale = 1 }, new ElementOfHarmony { Name = "Steadfastness", Scale = 1 }, new ElementOfHarmony { Name = "Perseverance", Scale = 1 },
        new ElementOfHarmony { Name = "Thoughtfulness", Scale = 1 }, new ElementOfHarmony { Name = "Courage", Scale = 1 }, new ElementOfHarmony { Name = "Yielding", Scale = 1 },
        new ElementOfHarmony { Name = "Faithfulness", Scale = 1 }, new ElementOfHarmony { Name = "Laughter", Scale = 1 }, new ElementOfHarmony { Name = "Graciousness", Scale = 1 },
        new ElementOfHarmony { Name = "Creativity", Scale = 1 }, new ElementOfHarmony { Name = "Affability", Scale = 1 }, new ElementOfHarmony { Name = "Cordiality", Scale = 1 },
        new ElementOfHarmony { Name = "Altruism", Scale = 1 }, new ElementOfHarmony { Name = "Purposefulness", Scale = 1 }, new ElementOfHarmony { Name = "Regard", Scale = 1 },
        new ElementOfHarmony { Name = "Warmheartedness", Scale = .9 }, new ElementOfHarmony { Name = "Tactfulness", Scale = 1 }, new ElementOfHarmony { Name = "Hospitality", Scale = 1 },
        new ElementOfHarmony { Name = "Gentleness", Scale = 1 }, new ElementOfHarmony { Name = "Sensitivity", Scale = 1 }, new ElementOfHarmony { Name = "Determination", Scale = 1 },
        new ElementOfHarmony { Name = "Sociability", Scale = 1 }, new ElementOfHarmony { Name = "Good will", Scale = 1 }, new ElementOfHarmony { Name = "Reliability", Scale = 1 },
        new ElementOfHarmony { Name = "Bounteousness", Scale = 1 }, new ElementOfHarmony { Name = "Beneficence", Scale = 1 }, new ElementOfHarmony { Name = "Dependability", Scale = 1 },
        new ElementOfHarmony { Name = "Affinity", Scale = 1 }, new ElementOfHarmony { Name = "Munificence", Scale = 1 }, new ElementOfHarmony { Name = "Attentiveness", Scale = 1 },
        new ElementOfHarmony { Name = "Fidelity", Scale = 1 }, new ElementOfHarmony { Name = "Honesty", Scale = 1 }, new ElementOfHarmony { Name = "Bravery", Scale = 1 },
        new ElementOfHarmony { Name = "Resoluteness", Scale = 1 }, new ElementOfHarmony { Name = "Helpfulness", Scale = 1 }, new ElementOfHarmony { Name = "Decorum", Scale = 1 },
        new ElementOfHarmony { Name = "Decency", Scale = 1 }, new ElementOfHarmony { Name = "Open-mindedness", Scale = .9 }, new ElementOfHarmony { Name = "Good faith", Scale = 1 },
        new ElementOfHarmony { Name = "Dauntlessness", Scale = 1 }, new ElementOfHarmony { Name = "Impartiality", Scale = 1 }, new ElementOfHarmony { Name = "Benignity", Scale = 1 },
        new ElementOfHarmony { Name = "Courteousness", Scale = 1 }, new ElementOfHarmony { Name = "Uprightness", Scale = 1 }, new ElementOfHarmony { Name = "Merriment", Scale = 1 },
        new ElementOfHarmony { Name = "Daring", Scale = 1 }, new ElementOfHarmony { Name = "Tenderheartedness", Scale = .8 }, new ElementOfHarmony { Name = "Morality", Scale = 1 },
        new ElementOfHarmony { Name = "Unity", Scale = 1 }, new ElementOfHarmony { Name = "Intrepidity", Scale = 1 }, new ElementOfHarmony { Name = "Condolence", Scale = 1 },
        new ElementOfHarmony { Name = "Liberality", Scale = 1 }, new ElementOfHarmony { Name = "Goodness", Scale = 1 }, new ElementOfHarmony { Name = "Compassion", Scale = 1 },
        new ElementOfHarmony { Name = "Openhandedness", Scale = .95 }, new ElementOfHarmony { Name = "Affection", Scale = 1 }, new ElementOfHarmony { Name = "Comradery", Scale = 1 },
        new ElementOfHarmony { Name = "Philanthropy", Scale = 1 }, new ElementOfHarmony { Name = "Gallantry", Scale = 1 }, new ElementOfHarmony { Name = "Concord", Scale = 1 },
        new ElementOfHarmony { Name = "Calmness", Scale = 1 }, new ElementOfHarmony { Name = "Fair-mindedness", Scale = .9 }, new ElementOfHarmony { Name = "Mildness", Scale = 1 },
        new ElementOfHarmony { Name = "Consideration", Scale = 1 }, new ElementOfHarmony { Name = "Candor", Scale = 1 }, new ElementOfHarmony { Name = "Humility", Scale = 1 },
        new ElementOfHarmony { Name = "Mercy", Scale = 1 }, new ElementOfHarmony { Name = "Evenhandedness", Scale = .95 }, new ElementOfHarmony { Name = "Hilarity", Scale = 1 },
        new ElementOfHarmony { Name = "Sincerity", Scale = 1 }, new ElementOfHarmony { Name = "Wholeheartedness", Scale = .85 }, new ElementOfHarmony { Name = "Equity", Scale = 1 },
        new ElementOfHarmony { Name = "Justice", Scale = 1 }, new ElementOfHarmony { Name = "Wholesomeness", Scale = 1 }, new ElementOfHarmony { Name = "Benevolence", Scale = 1 },
        new ElementOfHarmony { Name = "Clemency", Scale = 1 }, new ElementOfHarmony { Name = "Generosity", Scale = 1 }, new ElementOfHarmony { Name = "Patience", Scale = 1 },
        new ElementOfHarmony { Name = "Magnanimity", Scale = 1 }, new ElementOfHarmony { Name = "Forgiving", Scale = 1 }, new ElementOfHarmony { Name = "Imperturbability", Scale = .9 },
        new ElementOfHarmony { Name = "Sympathy", Scale = 1 }, new ElementOfHarmony { Name = "Charitableness", Scale = 1 }, new ElementOfHarmony { Name = "Kindness", Scale = 1 },
        new ElementOfHarmony { Name = "Truthfulness", Scale = 1 }, new ElementOfHarmony { Name = "Heedfulness", Scale = 1 }, new ElementOfHarmony { Name = "Genuineness", Scale = 1 },
        new ElementOfHarmony { Name = "Adaptability", Scale = 1 }, new ElementOfHarmony { Name = "Caring", Scale = 1 }, new ElementOfHarmony { Name = "Flexibility", Scale = 1 },
        new ElementOfHarmony { Name = "Understanding", Scale = 1 }, new ElementOfHarmony { Name = "Humanitarianism", Scale = .9 }, new ElementOfHarmony { Name = "Mindfulness", Scale = 1 },
        new ElementOfHarmony { Name = "Lenity", Scale = 1 }, new ElementOfHarmony { Name = "Comradeship", Scale = 1 }, new ElementOfHarmony { Name = "Venturesomeness", Scale = .9 },
        new ElementOfHarmony { Name = "Fairness", Scale = 1 }, new ElementOfHarmony { Name = "Equanimity", Scale = 1 }, new ElementOfHarmony { Name = "Candidness", Scale = 1 },
        new ElementOfHarmony { Name = "Earnestness", Scale = 1 }, new ElementOfHarmony { Name = "Amicability", Scale = 1 }, new ElementOfHarmony { Name = "Endurance", Scale = 1 },
        new ElementOfHarmony { Name = "Support", Scale = 1 }, new ElementOfHarmony { Name = "Comity", Scale = 1 }, new ElementOfHarmony { Name = "Tolerance", Scale = 1 },
        new ElementOfHarmony { Name = "Loyalty", Scale = 1 }, new ElementOfHarmony { Name = "Trustworthiness", Scale = .9 }, new ElementOfHarmony { Name = "Amusement", Scale = 1 },
        new ElementOfHarmony { Name = "Encouragement", Scale = 1 }, new ElementOfHarmony { Name = "Responsibility", Scale = 1 }, new ElementOfHarmony { Name = "Conscientiousness", Scale = .8 },
        new ElementOfHarmony { Name = "Inspiration", Scale = 1 }, new ElementOfHarmony { Name = "Straightforwardness", Scale = .75 }, new ElementOfHarmony { Name = "Kindheartedness", Scale = .9 },
        new ElementOfHarmony { Name = "Devotion", Scale = 1 }, new ElementOfHarmony { Name = "Friendliness", Scale = 1 }
    };

    static readonly int[][] _grid = new[] {
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
        public string Pony;
        public int SymbolPosition;
        public bool IsRowSymbol { get { return SymbolPosition % 28 >= 14; } }
        public int RowOrCol { get { return SymbolPosition >= 28 ? 13 - (SymbolPosition % 14) : SymbolPosition % 14; } }
        public override string ToString() { return string.Format("(X={0}, Y={1}, Pony={2} ({3}))", X, Y, Pony, IsRowSymbol ? "row" : "column"); }
    }

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        var rnd = RuleSeedable.GetRNG();
        Debug.LogFormat("[Friendship #{0}] Using rule seed: {1}", _moduleId, rnd.Seed);

        // Add extra randomess
        for (var i = rnd.Next(0, 10); i > 0; i--)
            rnd.NextDouble();

        var ponies = rnd.ShuffleFisherYates(_allPonyNames.ToArray()).Take(56).ToArray();
        var elementsOfHarmony = rnd.ShuffleFisherYates(_allElementsOfHarmony.ToArray()).Take(28).ToArray();
        var leftmost = rnd.Next(0, 2) == 1;
        var uppermost = rnd.Next(0, 2) == 1;

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
        var availablePonies = ponies.ToList();
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
            var fsIx = Rnd.Range(0, availablePonies.Count);
            var symbol = new SymbolInfo { X = x, Y = y, Pony = availablePonies[fsIx], SymbolPosition = Array.IndexOf(ponies, availablePonies[fsIx]) };
            availablePonies.RemoveAt(fsIx);

            // Remove the other friendship symbol from consideration that represents the same row/column as this one
            availablePonies.Remove(ponies[(13 - symbol.SymbolPosition % 14) + 14 * ((symbol.SymbolPosition / 14) ^ 2)]);

            // Determine whether this is a row or column symbol.
            if (symbol.IsRowSymbol)
            {
                rowSymbols++;
                if (rowSymbols == 3)
                {
                    // If we now have 3 row symbols, remove all the other row symbols from consideration.
                    availablePonies.RemoveAll(p => (Array.IndexOf(ponies, p) / 14) % 2 != 0);
                }
            }
            else
            {
                colSymbols++;
                if (colSymbols == 3)
                    // If we now have 3 column symbols, remove all the other column symbols from consideration.
                    availablePonies.RemoveAll(p => (Array.IndexOf(ponies, p) / 14) % 2 == 0);
            }

            friendshipSymbols.Add(symbol);
        }
        for (int i = 0; i < friendshipSymbols.Count; i++)
            logging.AppendLine(string.Format("[Friendship #{{0}}] Friendship symbol #{0}: {1}", i + 1, friendshipSymbols[i]));

        // Which column and row symbols should the expert disregard?
        var disregardCol = friendshipSymbols.Where(s => !s.IsRowSymbol && !friendshipSymbols.Any(s2 => s2 != s && s2.X == s.X)).OrderBy(s => leftmost ? s.X : -s.X).FirstOrDefault();
        if (disregardCol == null)
            goto tryAgain;
        logging.AppendLine(string.Format("[Friendship #{{0}}] Disregard column symbol {0}, leaving {1}", disregardCol.Pony, string.Join(" and ", friendshipSymbols.Where(s => !s.IsRowSymbol && s != disregardCol).Select(s => s.Pony).ToArray())));

        var disregardRow = friendshipSymbols.Where(s => s.IsRowSymbol && !friendshipSymbols.Any(s2 => s2 != s && s2.Y == s.Y)).OrderByDescending(s => uppermost ? s.Y : -s.Y).FirstOrDefault();
        if (disregardRow == null)
            goto tryAgain;

        logging.AppendLine(string.Format("[Friendship #{{0}}] Disregard row symbol {0}, leaving {1}", disregardRow.Pony, string.Join(" and ", friendshipSymbols.Where(s => s.IsRowSymbol && s != disregardRow).Select(s => s.Pony).ToArray())));
        Debug.LogFormat(logging.ToString(), _moduleId);

        // Which Elements of Harmony are at the intersections of the remaining columns and rows?
        var deducedElementsOfHarmony =
            friendshipSymbols.Where(s => !s.IsRowSymbol && s != disregardCol).SelectMany(cs =>
            friendshipSymbols.Where(s => s.IsRowSymbol && s != disregardRow).Select(rs => _grid[rs.RowOrCol][cs.RowOrCol])).Select(ix => elementsOfHarmony[ix]).ToArray();
        Debug.LogFormat("[Friendship #{0}] The potential Elements of Harmony are: {1}", _moduleId, string.Join(", ", deducedElementsOfHarmony.Select(eoh => eoh.Name).ToArray()));

        // On the bomb, display 6 wrong Elements of Harmony...
        var displayedElementsOfHarmony = new List<ElementOfHarmony>();
        var availableElementsOfHarmony = elementsOfHarmony.Where(e => !deducedElementsOfHarmony.Contains(e)).ToList();
        for (int i = 0; i < 6; i++)
        {
            var ix = Rnd.Range(0, availableElementsOfHarmony.Count);
            displayedElementsOfHarmony.Add(availableElementsOfHarmony[ix]);
            availableElementsOfHarmony.RemoveAt(ix);
        }
        // ... plus one of the correct ones in a random place
        var correctElementOfHarmony = deducedElementsOfHarmony[Rnd.Range(0, 4)];
        _correctElementOfHarmonyIndex = Rnd.Range(0, displayedElementsOfHarmony.Count + 1);
        displayedElementsOfHarmony.Insert(_correctElementOfHarmonyIndex, correctElementOfHarmony);
        _displayedElementsOfHarmony = displayedElementsOfHarmony.Select(eoh => eoh.Name).ToArray();

        for (int i = 0; i < 7; i++)
        {
            ElementsOfHarmony[i].text = displayedElementsOfHarmony[i].Name;
            ElementsOfHarmony[i].transform.localScale = new Vector3(displayedElementsOfHarmony[i].ScaleX, .005f, .005f);
        }

        // This is used by the TP support only
        _elementsOfHarmony = elementsOfHarmony.Select(eoh => eoh.Name).ToArray();

        Debug.LogFormat("[Friendship #{1}] Showing Elements of Harmony: {0}", string.Join(", ", _displayedElementsOfHarmony.ToArray()), _moduleId);
        Debug.LogFormat("[Friendship #{1}] Correct Element of Harmony: {0}", correctElementOfHarmony.Name, _moduleId);

        // Create the GameObjects to display the friendship symbols on the module.
        foreach (var friendshipSymbol in friendshipSymbols)
        {
            var graphic = Instantiate(FsTemplate);
            graphic.name = friendshipSymbol.Pony;
            graphic.transform.parent = FsScreen.transform;
            graphic.transform.localPosition = new Vector3(friendshipSymbol.X * .035f / 3 - .07f, 0.0001f, friendshipSymbol.Y * .035f / 3 - .022f);
            graphic.transform.localEulerAngles = new Vector3(90, 0, 0);
            graphic.transform.localScale = new Vector3(.035f, .035f, .035f);
            graphic.material.mainTexture = FriendshipSymbols.First(fs => fs.name == friendshipSymbol.Pony);
        }
        Destroy(FsTemplate.gameObject);

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
        _selectedElementOfHarmonyIndex = (_selectedElementOfHarmonyIndex + (up ? 6 : 1)) % 7;

        if (!_isCoroutineRunning)
            StartCoroutine(cylinderRotation());
    }

    private IEnumerator cylinderRotation()
    {
        _isCoroutineRunning = true;
        var rotationStart = FsCylinder.transform.localRotation;
        var elapsed = 0f;
        var selectionStart = _selectedElementOfHarmonyIndex;
        while (elapsed < _rotationAnimationDuration)
        {
            yield return null;

            if (_selectedElementOfHarmonyIndex != selectionStart)
            {
                selectionStart = _selectedElementOfHarmonyIndex;
                rotationStart = FsCylinder.transform.localRotation;
                elapsed = 0;
            }

            elapsed += Time.deltaTime;
            FsCylinder.transform.localRotation = Quaternion.Slerp(rotationStart, _cylinderRotations[_selectedElementOfHarmonyIndex], Easing.OutSine(elapsed, 0, 1, _rotationAnimationDuration));
        }
        FsCylinder.transform.localRotation = _cylinderRotations[_selectedElementOfHarmonyIndex];
        _isCoroutineRunning = false;
    }

    private void handleSubmit()
    {
        BtnSubmit.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BtnSubmit.transform);
        if (_isSolved)
            return;

        Debug.LogFormat("[Friendship #{2}] You selected {0}; correct is {1}.", _displayedElementsOfHarmony[_selectedElementOfHarmonyIndex], _displayedElementsOfHarmony[_correctElementOfHarmonyIndex], _moduleId);
        if (_selectedElementOfHarmonyIndex == _correctElementOfHarmonyIndex)
        {
            Debug.LogFormat("[Friendship #{0}] Yay!", _moduleId);
            Module.HandlePass();
            Audio.PlaySoundAtTransform("Yay", Module.transform);
            _isSolved = true;
        }
        else
            Module.HandleStrike();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Submit the desired Element of Harmony with “!{0} submit Fairness Conscientiousness Kindness Authenticity”.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        var pieces = command.ToLowerInvariant().Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (pieces.Length == 1 && pieces[0] == "cycle")
        {
            yield return null;
            for (int i = 0; i < 7; i++)
            {
                if (i > 0)
                    yield return new WaitForSeconds(1f);
                yield return BtnDown;
                yield return new WaitForSeconds(.1f);
                yield return BtnDown;
            }
        }
        else if (pieces.Length >= 2 && pieces[0] == "submit")
        {
            yield return null;

            var faulty = pieces.Skip(1).FirstOrDefault(str => !_elementsOfHarmony.Any(eoh => eoh.Equals(str, StringComparison.InvariantCultureIgnoreCase)));
            if (faulty != null)
            {
                yield return string.Format("sendtochat Dear Princess Celestia, today I learned that “{0}” is a new Element of Harmony. Your faithful student, Twilight Sparkle", faulty);
                yield return string.Format("sendtochat Dear Twilight Sparkle, it does seem that you are mistaken. The 28 Elements of Harmony are: {0}. Your mentor, Princess Celestia", string.Join(", ", _elementsOfHarmony));
                yield break;
            }

            for (int i = 0; i < 7; i++)
            {
                if (i > 0)
                {
                    BtnDown.OnInteract();
                    yield return new WaitForSeconds(.25f);
                }

                if (pieces.Skip(1).Any(p => p.Equals(_displayedElementsOfHarmony[_selectedElementOfHarmonyIndex], StringComparison.InvariantCultureIgnoreCase)))
                {
                    yield return new WaitForSeconds(.25f);
                    BtnSubmit.OnInteract();
                    yield break;
                }
            }

            yield return "unsubmittablepenalty";
        }
    }
}
