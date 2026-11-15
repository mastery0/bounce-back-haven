using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class PlayerAnimationSetup
{
    private const string TexturePath = "Assets/sprite/character-spritesheet_LOCO.png";
    private const string AnimationFolder = "Assets/Animations";
    private const string IdlePath = AnimationFolder + "/Player_Idle.anim";
    private const string WalkPath = AnimationFolder + "/Player_Walk.anim";
    private const string JumpPath = AnimationFolder + "/Player_Jump.anim";
    private const string ControllerPath = AnimationFolder + "/Player.controller";

    private const int CellSize = 64;

    [MenuItem("Tools/Bounce Back Haven/Setup Player Animations")]
    public static void Setup()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Esci dal Play Mode prima di eseguire il setup delle animazioni.");
            return;
        }

        ReSliceTexture();
        AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);

        var sprites = AssetDatabase.LoadAllAssetsAtPath(TexturePath)
            .OfType<Sprite>()
            .ToDictionary(s => s.name);

        Sprite[] idleFrames = GetFrames(sprites, "player_idle", 2);
        Sprite[] walkFrames = GetFrames(sprites, "player_walk", 9);
        Sprite[] jumpFrames = GetFrames(sprites, "player_jump", 5);

        if (idleFrames == null || walkFrames == null || jumpFrames == null)
        {
            Debug.LogError("Impossibile trovare gli sprite player_idle_*, player_walk_* o player_jump_*. Setup annullato.");
            return;
        }

        EnsureFolder(AnimationFolder);

        AnimationClip idleClip = BuildClip(IdlePath, "Player_Idle", idleFrames, 3f, true);
        AnimationClip walkClip = BuildClip(WalkPath, "Player_Walk", walkFrames, 12f, true);
        AnimationClip jumpClip = BuildClip(JumpPath, "Player_Jump", jumpFrames, 12f, false);

        BuildController(idleClip, walkClip, jumpClip);
        WirePlayer(idleFrames[0]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Setup animazioni Player completato! Clip in " + AnimationFolder + ".");
    }

    private static void ReSliceTexture()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        if (importer == null)
        {
            Debug.LogError("Texture non trovata: " + TexturePath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32f;

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var rects = new List<SpriteRect>();

        // Idle: 2 frame, bitmap riga 1600 -> Unity y = 3456 - 1 - 1663 = 1792
        AddFrames(rects, "player_idle", 2, 1792);
        // Walk: 9 frame, bitmap riga 704 -> Unity y = 3456 - 1 - 767 = 2688
        AddFrames(rects, "player_walk", 9, 2688);
        // Jump: 5 frame, bitmap riga 1856 -> Unity y = 3456 - 1 - 1919 = 1536
        AddFrames(rects, "player_jump", 5, 1536);

        dataProvider.SetSpriteRects(rects.ToArray());
        dataProvider.Apply();

        importer.SaveAndReimport();
    }

    private static void AddFrames(List<SpriteRect> rects, string prefix, int count, int unityY)
    {
        for (int i = 0; i < count; i++)
        {
            rects.Add(new SpriteRect
            {
                name = prefix + "_" + i,
                rect = new Rect(i * CellSize, unityY, CellSize, CellSize),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = Vector4.zero,
                spriteID = GUID.Generate()
            });
        }
    }

    private static Sprite[] GetFrames(Dictionary<string, Sprite> sprites, string prefix, int count)
    {
        var frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            if (!sprites.TryGetValue(prefix + "_" + i, out frames[i]))
            {
                Debug.LogError("Sprite mancante: " + prefix + "_" + i);
                return null;
            }
        }
        return frames;
    }

    private static AnimationClip BuildClip(string path, string name, Sprite[] frames, float fps, bool loop)
    {
        var clip = new AnimationClip { name = name, frameRate = fps };

        var binding = new EditorCurveBinding
        {
            path = "",
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };

        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = frames[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        settings.stopTime = frames.Length / fps;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Clamp;

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    private static void BuildController(AnimationClip idle, AnimationClip walk, AnimationClip jump)
    {
        AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        AnimatorState idleState = sm.AddState("Idle");
        idleState.motion = idle;

        AnimatorState walkState = sm.AddState("Walk");
        walkState.motion = walk;

        AnimatorState jumpState = sm.AddState("Jump");
        jumpState.motion = jump;

        sm.defaultState = idleState;

        AddTransition(sm, idleState,
            (AnimatorConditionMode.If, 0f, "Grounded"),
            (AnimatorConditionMode.Less, 0.1f, "Speed"));

        AddTransition(sm, walkState,
            (AnimatorConditionMode.If, 0f, "Grounded"),
            (AnimatorConditionMode.Greater, 0.1f, "Speed"));

        AddTransition(sm, jumpState,
            (AnimatorConditionMode.IfNot, 0f, "Grounded"));
    }

    private static void AddTransition(AnimatorStateMachine sm, AnimatorState destination,
        params (AnimatorConditionMode mode, float threshold, string parameter)[] conditions)
    {
        AnimatorStateTransition transition = sm.AddAnyStateTransition(destination);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.canTransitionToSelf = false;

        foreach (var condition in conditions)
        {
            transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
        }
    }

    private static void WirePlayer(Sprite initialSprite)
    {
        Player_Controller controller = Object.FindAnyObjectByType<Player_Controller>();
        GameObject player = controller != null ? controller.gameObject : GameObject.Find("Player");

        if (player == null)
        {
            Debug.LogError("Player non trovato nella scena aperta. Apri la scena di gioco e riprova.");
            return;
        }

        Animator animator = player.GetComponent<Animator>();
        if (animator == null) animator = player.AddComponent<Animator>();

        animator.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        animator.applyRootMotion = false;

        if (player.GetComponent<PlayerAnimator>() == null)
            player.AddComponent<PlayerAnimator>();

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = initialSprite;

        EditorSceneManager.MarkSceneDirty(player.scene);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
