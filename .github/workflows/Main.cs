using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MelonLoader;
using UnityEngine;
using UnityEngine.XR;

[assembly: MelonInfo(typeof(OllieGillaMenu.Main), "absolute ollie - gilla bench", "1.0.0", "ollie (built by olivia)")]
[assembly: MelonGame(null, null)]

namespace OllieGillaMenu
{
    public class Main : MelonMod
    {
        public static Main I;
        public Menu menu;
        public FeatureBank feats;
        public Theme theme = Theme.PurpleHaze();

        public override void OnInitializeMelon()
        {
            I = this;
            MelonLogger.Msg("absolute ollie - door open, .gg/ojailbreaks");
        }

        public override void OnUpdate()
        {
            if (menu == null) { TryBuild(); return; }
            InputX.Tick();
            if (InputX.YPressed) menu.Toggle();
            feats?.Tick();
            menu.Tick();
        }

        void TryBuild()
        {
            if (Camera.main == null) return;
            feats = new FeatureBank();
            menu = new Menu(theme, feats);
            menu.Build();
            MelonLogger.Msg("menu welded - press Y on left touch");
        }
    }

    public static class InputX
    {
        static InputDevice L, R;
        public static bool YPressed, StickClick, GripR, APressed, BPressed;
        public static Vector2 LStick;
        public static Vector3 LPos, RPos;
        public static Quaternion LRot;
        static bool _y, _a, _b, _st;

        public static void Tick()
        {
            if (!L.isValid) L = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (!R.isValid) R = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            bool y = false, a = false, b = false, st = false;
            float gf = 0f;
            L.TryGetFeatureValue(CommonUsages.secondaryButton, out y);
            R.TryGetFeatureValue(CommonUsages.primaryButton, out a);
            R.TryGetFeatureValue(CommonUsages.secondaryButton, out b);
            L.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out st);
            R.TryGetFeatureValue(CommonUsages.grip, out gf);

            YPressed = y && !_y;
            APressed = a && !_a;
            BPressed = b && !_b;
            StickClick = st && !_st;
            GripR = gf > 0.6f;
            _y = y; _a = a; _b = b; _st = st;

            L.TryGetFeatureValue(CommonUsages.primary2DAxis, out LStick);
            L.TryGetFeatureValue(CommonUsages.devicePosition, out LPos);
            R.TryGetFeatureValue(CommonUsages.devicePosition, out RPos);
            L.TryGetFeatureValue(CommonUsages.deviceRotation, out LRot);
        }
    }

    public class Theme
    {
        public Color panel = new Color32(0x4B, 0x1F, 0x8F, 230);
        public Color panelEdge = new Color32(0xB9, 0x6B, 0xFF, 255);
        public Color button = new Color32(0x6A, 0x2C, 0xC0, 230);
        public Color buttonHi = new Color32(0xE8, 0x9A, 0xFF, 255);
        public Color accent = new Color32(0xFF, 0x7A, 0xF0, 255);
        public Color text = Color.white;
        public string name = "Purple Haze";
        public bool rgb;

        public static Theme PurpleHaze() { return new Theme(); }
        public static Theme AcidLime()
        {
            var t = new Theme();
            t.panel = new Color32(0x1E, 0x4D, 0x1A, 230);
            t.panelEdge = new Color32(0x9C, 0xFF, 0x66, 255);
            t.button = new Color32(0x2A, 0x6B, 0x25, 230);
            t.buttonHi = new Color32(0xCB, 0xFF, 0x8A, 255);
            t.accent = new Color32(0xE7, 0xFF, 0x3B, 255);
            t.name = "Acid Lime";
            return t;
        }
        public static Theme BloodRail()
        {
            var t = new Theme();
            t.panel = new Color32(0x50, 0x12, 0x12, 230);
            t.panelEdge = new Color32(0xFF, 0x4D, 0x4D, 255);
            t.button = new Color32(0x7A, 0x1E, 0x1E, 230);
            t.buttonHi = new Color32(0xFF, 0x9A, 0x9A, 255);
            t.accent = new Color32(0xFF, 0xD5, 0x00, 255);
            t.name = "Blood Rail";
            return t;
        }
    }

    public class Menu
    {
        readonly Theme t;
        readonly FeatureBank f;
        GameObject root, leftPanel, rightPanel;
        readonly List<ButtonX> buttons = new List<ButtonX>();
        int cursor;
        bool open;

        public string[] cats = { "Settings", "Movement", "Player", "Safety", "Visual", "Fun", "Master", "Advantage", "Overpowered", "Projectiles" };
        public int cat = 1;

        public Menu(Theme theme, FeatureBank bank) { t = theme; f = bank; }

        public void Toggle()
        {
            open = !open;
            root.SetActive(open);
            Haptic.Pulse(XRNode.LeftHand, 0.4f, 0.03f);
        }

        public void Build()
        {
            root = new GameObject("OllieMenuRoot");
            root.SetActive(false);
            leftPanel = Panel("L", new Vector3(-0.09f, 0.02f, 0.18f), Quaternion.Euler(-30f, -20f, 0f));
            rightPanel = Panel("R", new Vector3(0.07f, 0.02f, 0.18f), Quaternion.Euler(-30f, 10f, 0f));
            BuildHeader(leftPanel.transform);
            BuildLeftRows(leftPanel.transform);
            BuildCategories(rightPanel.transform);
        }

        GameObject Panel(string id, Vector3 lp, Quaternion lr)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            GameObject.Destroy(go.GetComponent<Collider>());
            go.name = "Panel_" + id;
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = lp;
            go.transform.localRotation = lr;
            go.transform.localScale = new Vector3(0.14f, 0.22f, 1f);
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = t.panel;
            go.GetComponent<MeshRenderer>().material = mat;

            var edge = GameObject.CreatePrimitive(PrimitiveType.Quad);
            GameObject.Destroy(edge.GetComponent<Collider>());
            edge.transform.SetParent(go.transform, false);
            edge.transform.localScale = new Vector3(1.08f, 1.05f, 1f);
            edge.transform.localPosition = new Vector3(0, 0, 0.001f);
            var em = new Material(Shader.Find("Sprites/Default"));
            em.color = t.panelEdge;
            edge.GetComponent<MeshRenderer>().material = em;
            return go;
        }

        void BuildHeader(Transform p)
        {
            Label(p, "Disconnect", new Vector3(0, 0.45f, -0.01f), 0.06f, t.accent);
            Label(p, "Juul", new Vector3(0, 0.37f, -0.01f), 0.14f, Color.white);
            Label(p, "<<<<<  >>>>>", new Vector3(0, 0.30f, -0.01f), 0.05f, Color.white);
            Label(p, "discord.gg/ojailbreaks", new Vector3(0, 0.25f, -0.01f), 0.04f, t.buttonHi);
        }

        void BuildLeftRows(Transform p)
        {
            string[] rows = {
                "Save Config", "Load Config", "Theme: " + t.name, "RGB Menu",
                "Fly (hold grip)", "ESP", "Platforms", "Nametags",
                "Low Gravity", "High Gravity", "Long Arms", "Noclip",
                "Disable All", "Reset"
            };
            float y = 0.18f;
            foreach (var r in rows)
            {
                string captured = r;
                buttons.Add(Row(p, r, y, delegate { OnSetting(captured); }));
                y -= 0.055f;
            }
        }

        void BuildCategories(Transform p)
        {
            float y = 0.42f;
            for (int i = 0; i < cats.Length; i++)
            {
                int idx = i;
                buttons.Add(Row(p, cats[i], y, delegate { cat = idx; }));
                y -= 0.055f;
            }
        }

        public void Tick()
        {
            if (!open) return;
            if (Mathf.Abs(InputX.LStick.y) > 0.6f)
                cursor = Mathf.Clamp(cursor + (InputX.LStick.y > 0 ? -1 : 1), 0, buttons.Count - 1);
            for (int i = 0; i < buttons.Count; i++) buttons[i].SetHi(i == cursor);
            if (InputX.StickClick && cursor < buttons.Count) buttons[cursor].Fire();

            root.transform.position = InputX.LPos;
            root.transform.rotation = InputX.LRot;

            if (t.rgb)
            {
                float h = (Time.time * 0.15f) % 1f;
                var c = Color.HSVToRGB(h, 0.85f, 1f);
                foreach (var b in buttons) b.SetAccent(c);
            }
        }

        void OnSetting(string row)
        {
            if (row.StartsWith("Theme:")) Cycle();
            else if (row == "RGB Menu") t.rgb = !t.rgb;
            else if (row == "Fly (hold grip)") f.flyToggle = !f.flyToggle;
            else if (row == "ESP") f.esp = !f.esp;
            else if (row == "Platforms") f.platforms = !f.platforms;
            else if (row == "Nametags") f.nameTags = !f.nameTags;
            else if (row == "Low Gravity") f.lowGrav = !f.lowGrav;
            else if (row == "High Gravity") f.highGrav = !f.highGrav;
            else if (row == "Long Arms") f.longArms = !f.longArms;
            else if (row == "Noclip") f.noclip = !f.noclip;
            else if (row == "Save Config") Config.Save(f, t);
            else if (row == "Load Config") Config.Load(f, t);
            else if (row == "Disable All") f.DisableAll();
            else if (row == "Reset") Main.I.theme = Theme.PurpleHaze();
        }

        void Cycle()
        {
            Main.I.theme = t.name == "Purple Haze" ? Theme.AcidLime()
                        : t.name == "Acid Lime" ? Theme.BloodRail()
                        : Theme.PurpleHaze();
        }

        ButtonX Row(Transform parent, string text, float y, Action onPress)
        {
            var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            GameObject.Destroy(bg.GetComponent<Collider>());
            bg.transform.SetParent(parent, false);
            bg.transform.localPosition = new Vector3(0, y, -0.01f);
            bg.transform.localScale = new Vector3(0.95f, 0.045f, 1f);
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = t.button;
            bg.GetComponent<MeshRenderer>().material = mat;
            Label(bg.transform, text, new Vector3(0, 0, -0.01f), 0.5f, t.text);
            return new ButtonX { bg = bg.GetComponent<MeshRenderer>(), press = onPress, theme = t };
        }

        TextMesh Label(Transform parent, string s, Vector3 lp, float sz, Color col)
        {
            var go = new GameObject("lbl");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = lp;
            go.transform.localScale = Vector3.one * sz;
            var tm = go.AddComponent<TextMesh>();
            tm.text = s;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = col;
            tm.characterSize = 0.2f;
            tm.fontSize = 48;
            return tm;
        }
    }

    public class ButtonX
    {
        public MeshRenderer bg;
        public Action press;
        public Theme theme;
        bool hi;
        public void SetHi(bool v) { hi = v; bg.material.color = v ? theme.buttonHi : theme.button; }
        public void SetAccent(Color c) { if (hi) bg.material.color = c; }
        public void Fire() { if (press != null) press(); Haptic.Pulse(XRNode.LeftHand, 0.6f, 0.05f); }
    }

    public class FeatureBank
    {
        public bool flyToggle, longArms, noclip, lowGrav, highGrav, nameTags, platforms, esp;
        public float speedMult = 1.6f;
        public Vector3? checkpoint;
        Transform _root;

        Transform Root()
        {
            if (_root != null) return _root;
            if (Camera.main != null)
            {
                var tr = Camera.main.transform;
                while (tr.parent != null) tr = tr.parent;
                _root = tr;
            }
            return _root;
        }

        public void DisableAll()
        {
            flyToggle = longArms = noclip = lowGrav = highGrav = nameTags = platforms = esp = false;
        }

        public void Tick()
        {
            var root = Root(); if (root == null) return;

            if (flyToggle && InputX.GripR)
            {
                var head = Camera.main.transform;
                root.position += head.forward * (0.08f * speedMult);
            }

            if (InputX.BPressed) checkpoint = root.position;
            if (InputX.APressed && checkpoint.HasValue) root.position = checkpoint.Value;

            Physics.gravity = lowGrav ? new Vector3(0, -2.5f, 0)
                            : highGrav ? new Vector3(0, -30f, 0)
                            : new Vector3(0, -9.81f, 0);

            if (platforms && InputX.GripR)
            {
                var q = GameObject.CreatePrimitive(PrimitiveType.Cube);
                q.transform.position = InputX.RPos + Vector3.down * 0.1f;
                q.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
                q.GetComponent<Renderer>().material.color = new Color(0.8f, 0.4f, 1f, 0.8f);
                GameObject.Destroy(q, 6f);
            }

            if (esp || nameTags)
            {
                var all = Resources.FindObjectsOfTypeAll<Component>();
                for (int i = 0; i < all.Length; i++)
                {
                    var c = all[i];
                    if (c == null) continue;
                    var n = c.GetType().Name;
                    if (n.IndexOf("Rig", StringComparison.OrdinalIgnoreCase) < 0 &&
                        n.IndexOf("Player", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    var tr = c.transform;
                    if (esp)
                    {
                        var rends = tr.GetComponentsInChildren<Renderer>();
                        for (int j = 0; j < rends.Length; j++)
                            rends[j].material.color = Color.magenta;
                    }
                    if (nameTags && tr.Find("_ollietag") == null)
                    {
                        var go = new GameObject("_ollietag");
                        go.transform.SetParent(tr, false);
                        go.transform.localPosition = new Vector3(0, 0.6f, 0);
                        var tm = go.AddComponent<TextMesh>();
                        tm.text = c.gameObject.name;
                        tm.anchor = TextAnchor.MiddleCenter;
                        tm.characterSize = 0.1f;
                        tm.color = Color.cyan;
                    }
                }
            }
        }
    }

    public static class Haptic
    {
        public static void Pulse(XRNode n, float amp, float dur)
        {
            var d = InputDevices.GetDeviceAtXRNode(n);
            HapticCapabilities cap;
            if (d.TryGetHapticCapabilities(out cap) && cap.supportsImpulse)
                d.SendHapticImpulse(0, amp, dur);
        }
    }

    public static class Config
    {
        public static string Path { get { return System.IO.Path.Combine(MelonUtils.UserDataDirectory, "ollie_gilla.json"); } }

        public static void Save(FeatureBank f, Theme t)
        {
            var s = "{\"esp\":" + f.esp.ToString().ToLower() +
                    ",\"fly\":" + f.flyToggle.ToString().ToLower() +
                    ",\"theme\":\"" + t.name + "\"}";
            File.WriteAllText(Path, s);
        }

        public static void Load(FeatureBank f, Theme t)
        {
            if (!File.Exists(Path)) return;
            var s = File.ReadAllText(Path);
            if (s.Contains("\"esp\":true")) f.esp = true;
            if (s.Contains("\"fly\":true")) f.flyToggle = true;
        }
    }
}
