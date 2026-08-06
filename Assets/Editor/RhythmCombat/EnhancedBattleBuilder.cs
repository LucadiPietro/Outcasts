#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class EnhancedBattleSceneInstaller
{
    const string BattleScenePath =
        "Assets/Scenes/Battle.unity";

    const string GeneratedRoot =
        "Assets/RhythmCombat/Generated/EnhancedBattle";

    const string NotePrefabPath =
        GeneratedRoot + "/BattleButton_Enhanced.prefab";

    const string InputIconRoot =
        "Assets/RhythmCombat/EnhancedBattle/InputIcons";

    const string KeyboardIconRoot =
        InputIconRoot + "/Keyboard";

    const string GamepadIconRoot =
        InputIconRoot + "/Gamepad";

    const string InstallationMarkerPath =
        GeneratedRoot + "/RuntimeRemapSpritesAndBinder_v3_3.installed.txt";

    static readonly string[] LegacyGeneratedInputAssets =
    {
        GeneratedRoot + "/BattleLaneActions.asset",
        GeneratedRoot + "/AttackLane1_Reference.asset",
        GeneratedRoot + "/AttackLane2_Reference.asset",
        GeneratedRoot + "/AttackLane3_Reference.asset",
        GeneratedRoot + "/DefenceLane1_Reference.asset",
        GeneratedRoot + "/DefenceLane2_Reference.asset",
        GeneratedRoot + "/DefenceLane3_Reference.asset"
    };

    static bool isQueued;

    sealed class RuntimeRemapUiBuildResult
    {
        public CanvasGroup CanvasGroup;
        public List<Button> KeyboardButtons = new List<Button>(6);
        public List<Button> GamepadButtons = new List<Button>(6);
        public List<TextMeshProUGUI> KeyboardLabels = new List<TextMeshProUGUI>(6);
        public List<TextMeshProUGUI> GamepadLabels = new List<TextMeshProUGUI>(6);
        public List<Image> KeyboardIcons = new List<Image>(6);
        public List<Image> GamepadIcons = new List<Image>(6);
        public TextMeshProUGUI StatusLabel;
    }

    static EnhancedBattleSceneInstaller()
    {
        QueueInstallation();
    }

    static void QueueInstallation()
    {
        if (isQueued)
        {
            return;
        }

        isQueued = true;
        EditorApplication.update += TryInstallWhenReady;
    }

    static void TryInstallWhenReady()
    {
        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(
                BattleScenePath) == null)
        {
            StopWaiting();
            Debug.LogWarning(
                "Battle Enhanced v3.3: non trovo " +
                BattleScenePath +
                ". L'installazione automatica non è stata eseguita.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<TextAsset>(
                InstallationMarkerPath) != null)
        {
            StopWaiting();
            return;
        }

        SceneSetup[] previousSetup =
            EditorSceneManager.GetSceneManagerSetup();

        for (int i = 0; i < previousSetup.Length; i++)
        {
            Scene scene =
                SceneManager.GetSceneByPath(previousSetup[i].path);

            if (scene.IsValid() && scene.isDirty)
            {
                return;
            }
        }

        StopWaiting();
        InstallIntoBattleScene(previousSetup);
    }

    static void StopWaiting()
    {
        EditorApplication.update -= TryInstallWhenReady;
        isQueued = false;
    }

    static void InstallIntoBattleScene(
        SceneSetup[] previousSetup)
    {
        try
        {
            EnsureFolder("Assets/RhythmCombat");
            EnsureFolder("Assets/RhythmCombat/Generated");
            EnsureFolder(GeneratedRoot);

            EnsureInputIconImportSettings();
            DeleteLegacyInputAssets();

            BattleButton notePrefab =
                BuildEnhancedNotePrefab();

            if (notePrefab == null)
            {
                throw new InvalidOperationException(
                    "Creazione del prefab BattleButton_Enhanced fallita.");
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    BattleScenePath,
                    OpenSceneMode.Single);

            BattleManager battleManager =
                Object.FindObjectOfType<BattleManager>();

            BMBattleManager bmBattleManager =
                Object.FindObjectOfType<BMBattleManager>();

            BattleRhythmChartRunner chartRunner =
                Object.FindObjectOfType<BattleRhythmChartRunner>();

            RectTransform panel =
                FindNamedRect("Panel");

            RectTransform lineCenter =
                FindNamedRect("LineCenter");

            RectTransform lineUp =
                FindNamedRect("LineUp");

            RectTransform lineDown =
                FindNamedRect("LineDown");

            if (battleManager == null ||
                bmBattleManager == null ||
                panel == null)
            {
                throw new InvalidOperationException(
                    "Riferimenti principali mancanti: " +
                    "BattleManager, BMBattleManager o Panel.");
            }

            BattleLaneInputRouter inputRouter =
                battleManager.GetComponent<BattleLaneInputRouter>();

            List<BattleLaneInputRouter.LaneBinding> savedLaneBindings =
                inputRouter != null
                    ? inputRouter.CreateEditorBindingsSnapshot()
                    : null;

            List<BattleLaneInputRouter.KeyboardIconBinding> savedKeyboardIcons =
                inputRouter != null
                    ? inputRouter.CreateEditorKeyboardIconsSnapshot()
                    : null;

            List<BattleLaneInputRouter.GamepadIconBinding> savedGamepadIcons =
                inputRouter != null
                    ? inputRouter.CreateEditorGamepadIconsSnapshot()
                    : null;

            DeleteExistingEnhancementRoot(panel);

            bmBattleManager.ConfigureEditorReferences(
                panel,
                lineCenter,
                lineUp,
                lineDown,
                notePrefab);

            DisableLegacyHardcodedInput(battleManager);

            GameObject enhancementRoot =
                new GameObject(
                    "EnhancedRhythmSystems",
                    typeof(RectTransform));

            RectTransform enhancementRect =
                enhancementRoot.GetComponent<RectTransform>();

            enhancementRect.SetParent(panel, false);
            enhancementRect.anchorMin = Vector2.zero;
            enhancementRect.anchorMax = Vector2.one;
            enhancementRect.offsetMin = Vector2.zero;
            enhancementRect.offsetMax = Vector2.zero;
            enhancementRect.SetAsLastSibling();

            BattleVisualFeedbackController visualController =
                enhancementRoot.AddComponent<
                    BattleVisualFeedbackController>();

            BattleChordFlashController chordController =
                enhancementRoot.AddComponent<
                    BattleChordFlashController>();

            BattleRuntimeRemapMenu runtimeRemapMenu =
                enhancementRoot.AddComponent<
                    BattleRuntimeRemapMenu>();

            if (inputRouter == null)
            {
                inputRouter =
                    battleManager.gameObject.AddComponent<
                        BattleLaneInputRouter>();
            }

            BattleCharacterAutoBinder characterBinder =
                battleManager.GetComponent<BattleCharacterAutoBinder>();

            if (characterBinder == null)
            {
                characterBinder =
                    battleManager.gameObject.AddComponent<
                        BattleCharacterAutoBinder>();
            }

            characterBinder.ConfigureEditor(battleManager);

            List<BattleLaneFeedbackSlot> feedbackSlots =
                BuildFeedbackSlots(
                    enhancementRect,
                    bmBattleManager);

            visualController.ConfigureEditor(feedbackSlots);

            List<ChordFlashLink> chordLinks =
                BuildChordLinkPool(
                    enhancementRect,
                    36);

            chordController.ConfigureEditor(
                enhancementRect,
                chordLinks);

            RuntimeRemapUiBuildResult remapUi =
                BuildRuntimeRemapUi(enhancementRect);

            inputRouter.ConfigureEditor(
                battleManager,
                visualController,
                BuildLaneBindings(bmBattleManager, savedLaneBindings));

            inputRouter.ConfigureEditorIconLibrary(
                BuildKeyboardIconBindings(savedKeyboardIcons),
                BuildGamepadIconBindings(savedGamepadIcons));

            runtimeRemapMenu.ConfigureEditor(
                inputRouter,
                bmBattleManager,
                chartRunner,
                remapUi.CanvasGroup,
                remapUi.KeyboardButtons,
                remapUi.GamepadButtons,
                remapUi.KeyboardLabels,
                remapUi.GamepadLabels,
                remapUi.KeyboardIcons,
                remapUi.GamepadIcons,
                remapUi.StatusLabel);

            if (chartRunner != null)
            {
                chartRunner.ConfigureEditorReferences(
                    bmBattleManager,
                    chordController);
            }

            EditorUtility.SetDirty(battleManager);
            EditorUtility.SetDirty(bmBattleManager);
            EditorUtility.SetDirty(visualController);
            EditorUtility.SetDirty(chordController);
            EditorUtility.SetDirty(runtimeRemapMenu);
            EditorUtility.SetDirty(inputRouter);
            EditorUtility.SetDirty(characterBinder);

            if (chartRunner != null)
            {
                EditorUtility.SetDirty(chartRunner);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            File.WriteAllText(
                InstallationMarkerPath,
                "Battle Enhanced v3.3 runtime remap, dynamic sprites and deferred character binding installed.");

            AssetDatabase.ImportAsset(InstallationMarkerPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Battle Enhanced v3.3 installata direttamente in " +
                BattleScenePath +
                ". Esc o Start aprono la rimappatura durante il gioco; " +
                "le note simultanee restano collegate durante il movimento.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Battle Enhanced v3.3: installazione automatica fallita.\n" +
                exception);
        }
        finally
        {
            if (previousSetup != null &&
                previousSetup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }
    }

    static void DeleteLegacyInputAssets()
    {
        for (int i = 0;
             i < LegacyGeneratedInputAssets.Length;
             i++)
        {
            string path = LegacyGeneratedInputAssets[i];

            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
    }

    static BattleButton BuildEnhancedNotePrefab()
    {
        AssetDatabase.DeleteAsset(
            NotePrefabPath);

        GameObject root =
            new GameObject(
                "BattleButton_Enhanced",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(BoxCollider2D),
                typeof(Rigidbody2D),
                typeof(BattleButton));

        RectTransform rootRect =
            root.GetComponent<RectTransform>();

        rootRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rootRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rootRect.pivot =
            new Vector2(0.5f, 0.5f);

        rootRect.sizeDelta =
            new Vector2(50f, 50f);

        Image mainImage =
            root.GetComponent<Image>();

        mainImage.preserveAspect = true;
        mainImage.raycastTarget = false;

        BoxCollider2D collider =
            root.GetComponent<BoxCollider2D>();

        collider.isTrigger = true;
        collider.size =
            new Vector2(50f, 50f);

        Rigidbody2D rigidbody =
            root.GetComponent<Rigidbody2D>();

        rigidbody.bodyType =
            RigidbodyType2D.Kinematic;

        rigidbody.gravityScale = 0f;

        GameObject labelObject =
            new GameObject(
                "InputLabel",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.SetParent(rootRect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label =
            labelObject.GetComponent<
                TextMeshProUGUI>();

        label.alignment =
            TextAlignmentOptions.Center;

        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false;
        label.enabled = false;

        GameObject bodyObject =
            new GameObject(
                "HoldBody",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        RectTransform bodyRect =
            bodyObject.GetComponent<RectTransform>();

        bodyRect.SetParent(rootRect, false);
        bodyRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        bodyRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        bodyRect.pivot =
            new Vector2(0.5f, 0.5f);

        bodyRect.sizeDelta =
            new Vector2(28f, 1f);

        Image bodyImage =
            bodyObject.GetComponent<Image>();

        bodyImage.raycastTarget = false;
        bodyObject.SetActive(false);
        bodyRect.SetAsFirstSibling();

        GameObject tailObject =
            new GameObject(
                "HoldTail",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        RectTransform tailRect =
            tailObject.GetComponent<RectTransform>();

        tailRect.SetParent(rootRect, false);
        tailRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        tailRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        tailRect.pivot =
            new Vector2(0.5f, 0.5f);

        tailRect.sizeDelta =
            new Vector2(38f, 38f);

        Image tailImage =
            tailObject.GetComponent<Image>();

        tailImage.raycastTarget = false;
        tailObject.SetActive(false);

        BattleButton button =
            root.GetComponent<BattleButton>();

        button.ConfigurePrefabVisuals(
            mainImage,
            label,
            bodyRect,
            bodyImage,
            tailRect,
            tailImage);

        GameObject prefab =
            PrefabUtility.SaveAsPrefabAsset(
                root,
                NotePrefabPath);

        Object.DestroyImmediate(root);

        return prefab != null
            ? prefab.GetComponent<BattleButton>()
            : null;
    }

    static List<BattleLaneFeedbackSlot>
        BuildFeedbackSlots(
            RectTransform parent,
            BMBattleManager manager)
    {
        List<BattleLaneFeedbackSlot> slots =
            new List<BattleLaneFeedbackSlot>(6);

        TMP_FontAsset font =
            Object.FindObjectOfType<
                TextMeshProUGUI>(true)?.font;

        for (int lane = 0; lane < 6; lane++)
        {
            BMButtonPrefab.Cell cell =
                (BMButtonPrefab.Cell)lane;

            GameObject slotObject =
                new GameObject(
                    "PressFeedback_" + cell,
                    typeof(RectTransform),
                    typeof(CanvasGroup),
                    typeof(CanvasRenderer),
                    typeof(LanePressFeedbackGraphic),
                    typeof(BattleLaneFeedbackSlot));

            RectTransform rect =
                slotObject.GetComponent<RectTransform>();

            rect.SetParent(parent, false);
            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.sizeDelta =
                new Vector2(160f, 160f);

            rect.anchoredPosition =
                manager.GetLaneTargetAnchoredPosition(
                    cell);

            CanvasGroup canvasGroup =
                slotObject.GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            LanePressFeedbackGraphic ring =
                slotObject.GetComponent<
                    LanePressFeedbackGraphic>();

            ring.raycastTarget = false;

            GameObject labelObject =
                new GameObject(
                    "JudgmentLabel",
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI));

            RectTransform labelRect =
                labelObject.GetComponent<
                    RectTransform>();

            labelRect.SetParent(rect, false);
            labelRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            labelRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            labelRect.pivot =
                new Vector2(0.5f, 0.5f);

            labelRect.anchoredPosition =
                cell <= BMButtonPrefab.Cell.Cell3
                    ? new Vector2(0f, 92f)
                    : new Vector2(0f, -92f);

            labelRect.sizeDelta =
                new Vector2(210f, 62f);

            TextMeshProUGUI label =
                labelObject.GetComponent<
                    TextMeshProUGUI>();

            label.font = font;
            label.fontSize = 25f;
            label.fontStyle = FontStyles.Bold;
            label.alignment =
                TextAlignmentOptions.Center;

            label.raycastTarget = false;

            BattleLaneFeedbackSlot slot =
                slotObject.GetComponent<
                    BattleLaneFeedbackSlot>();

            slot.ConfigureEditor(
                cell,
                canvasGroup,
                ring,
                label);

            slots.Add(slot);
        }

        return slots;
    }

    static List<ChordFlashLink>
        BuildChordLinkPool(
            RectTransform parent,
            int count)
    {
        GameObject poolObject =
            new GameObject(
                "ChordFlashPool",
                typeof(RectTransform));

        RectTransform poolRect =
            poolObject.GetComponent<RectTransform>();

        poolRect.SetParent(parent, false);
        poolRect.anchorMin = Vector2.zero;
        poolRect.anchorMax = Vector2.one;
        poolRect.offsetMin = Vector2.zero;
        poolRect.offsetMax = Vector2.zero;

        List<ChordFlashLink> links =
            new List<ChordFlashLink>(count);

        for (int i = 0; i < count; i++)
        {
            GameObject linkObject =
                new GameObject(
                    "ChordFlashLink_" + i,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(CanvasGroup),
                    typeof(ChordFlashLink));

            RectTransform linkRect =
                linkObject.GetComponent<
                    RectTransform>();

            linkRect.SetParent(poolRect, false);
            linkRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            linkRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            linkRect.pivot =
                new Vector2(0.5f, 0.5f);

            linkRect.sizeDelta =
                new Vector2(100f, 12f);

            Image image =
                linkObject.GetComponent<Image>();

            image.raycastTarget = false;

            CanvasGroup canvasGroup =
                linkObject.GetComponent<
                    CanvasGroup>();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            ChordFlashLink link =
                linkObject.GetComponent<
                    ChordFlashLink>();

            link.ConfigureEditor(
                linkRect,
                image,
                canvasGroup);

            linkObject.SetActive(false);
            links.Add(link);
        }

        return links;
    }

    static RuntimeRemapUiBuildResult BuildRuntimeRemapUi(
        RectTransform parent)
    {
        RuntimeRemapUiBuildResult result =
            new RuntimeRemapUiBuildResult();

        TMP_FontAsset font =
            Object.FindObjectOfType<TextMeshProUGUI>(true)?.font;

        GameObject overlayObject =
            new GameObject(
                "RuntimeRemapOverlay",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup));

        RectTransform overlayRect =
            overlayObject.GetComponent<RectTransform>();

        overlayRect.SetParent(parent, false);
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.SetAsLastSibling();

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0.015f, 0.02f, 0.035f, 0.90f);
        overlayImage.raycastTarget = true;

        result.CanvasGroup = overlayObject.GetComponent<CanvasGroup>();
        result.CanvasGroup.alpha = 0f;
        result.CanvasGroup.interactable = false;
        result.CanvasGroup.blocksRaycasts = false;

        GameObject panelObject =
            new GameObject(
                "RuntimeRemapPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        RectTransform panelRect =
            panelObject.GetComponent<RectTransform>();

        panelRect.SetParent(overlayRect, false);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(1120f, 760f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.055f, 0.065f, 0.09f, 0.98f);
        panelImage.raycastTarget = true;

        CreateRemapText(
            panelRect,
            "Title",
            "RIMAPPATURA COMANDI",
            font,
            new Vector2(0f, 320f),
            new Vector2(980f, 70f),
            38f,
            FontStyles.Bold);

        CreateRemapText(
            panelRect,
            "Subtitle",
            "Mouse oppure Frecce / D-Pad. Invio / A conferma. Backspace / X cancella.",
            font,
            new Vector2(0f, 270f),
            new Vector2(980f, 42f),
            21f,
            FontStyles.Normal);

        CreateRemapText(
            panelRect,
            "LaneHeader",
            "LANE",
            font,
            new Vector2(-405f, 218f),
            new Vector2(190f, 40f),
            23f,
            FontStyles.Bold);

        CreateRemapText(
            panelRect,
            "KeyboardHeader",
            "TASTIERA",
            font,
            new Vector2(-95f, 218f),
            new Vector2(300f, 40f),
            23f,
            FontStyles.Bold);

        CreateRemapText(
            panelRect,
            "GamepadHeader",
            "GAMEPAD",
            font,
            new Vector2(285f, 218f),
            new Vector2(300f, 40f),
            23f,
            FontStyles.Bold);

        string[] laneNames =
        {
            "Lane 1  Attack",
            "Lane 2  Attack",
            "Lane 3  Attack",
            "Lane 4  Defence",
            "Lane 5  Defence",
            "Lane 6  Defence"
        };

        for (int lane = 0; lane < 6; lane++)
        {
            float y = 158f - lane * 63f;

            CreateRemapText(
                panelRect,
                "LaneLabel_" + lane,
                laneNames[lane],
                font,
                new Vector2(-405f, y),
                new Vector2(220f, 52f),
                23f,
                FontStyles.Bold);

            TextMeshProUGUI keyboardLabel;
            Image keyboardIcon;
            Button keyboardButton = CreateRemapButton(
                panelRect,
                "KeyboardButton_" + lane,
                font,
                new Vector2(-95f, y),
                new Vector2(310f, 52f),
                out keyboardLabel,
                out keyboardIcon);

            TextMeshProUGUI gamepadLabel;
            Image gamepadIcon;
            Button gamepadButton = CreateRemapButton(
                panelRect,
                "GamepadButton_" + lane,
                font,
                new Vector2(285f, y),
                new Vector2(310f, 52f),
                out gamepadLabel,
                out gamepadIcon);

            result.KeyboardButtons.Add(keyboardButton);
            result.GamepadButtons.Add(gamepadButton);
            result.KeyboardLabels.Add(keyboardLabel);
            result.GamepadLabels.Add(gamepadLabel);
            result.KeyboardIcons.Add(keyboardIcon);
            result.GamepadIcons.Add(gamepadIcon);
        }

        result.StatusLabel = CreateRemapText(
            panelRect,
            "Status",
            "Seleziona una lane da rimappare.",
            font,
            new Vector2(0f, -248f),
            new Vector2(980f, 54f),
            22f,
            FontStyles.Bold);

        CreateRemapText(
            panelRect,
            "CloseHint",
            "ESC / START: chiudi e riprendi la battaglia",
            font,
            new Vector2(0f, -310f),
            new Vector2(980f, 44f),
            20f,
            FontStyles.Normal);

        return result;
    }

    static Button CreateRemapButton(
        RectTransform parent,
        string objectName,
        TMP_FontAsset font,
        Vector2 position,
        Vector2 size,
        out TextMeshProUGUI label,
        out Image icon)
    {
        GameObject buttonObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        GameObject iconObject =
            new GameObject(
                "BindingIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.SetParent(rect, false);
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(-112f, 0f);
        iconRect.sizeDelta = new Vector2(42f, 42f);

        icon = iconObject.GetComponent<Image>();
        icon.raycastTarget = false;
        icon.preserveAspect = true;
        icon.enabled = false;

        label = CreateRemapText(
            rect,
            "Label",
            "None",
            font,
            new Vector2(22f, 0f),
            new Vector2(size.x - 64f, size.y),
            23f,
            FontStyles.Bold);

        return button;
    }

    static TextMeshProUGUI CreateRemapText(
        RectTransform parent,
        string objectName,
        string text,
        TMP_FontAsset font,
        Vector2 position,
        Vector2 size,
        float fontSize,
        FontStyles fontStyle)
    {
        GameObject textObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

        RectTransform rect =
            textObject.GetComponent<RectTransform>();

        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI label =
            textObject.GetComponent<TextMeshProUGUI>();

        if (font != null)
        {
            label.font = font;
        }

        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        label.enableWordWrapping = false;

        return label;
    }

    static void EnsureInputIconImportSettings()
    {
        if (!AssetDatabase.IsValidFolder(InputIconRoot))
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets(
            "t:Texture2D",
            new[] { InputIconRoot });

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                continue;
            }

            bool changed =
                importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single ||
                importer.mipmapEnabled ||
                !importer.alphaIsTransparency ||
                importer.maxTextureSize != 128;

            if (!changed)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 128;
            importer.SaveAndReimport();
        }
    }

    static List<BattleLaneInputRouter.KeyboardIconBinding>
        BuildKeyboardIconBindings(
            List<BattleLaneInputRouter.KeyboardIconBinding> savedIcons)
    {
        var byKey =
            new Dictionary<Key, BattleLaneInputRouter.KeyboardIconBinding>();

        List<BattleLaneInputRouter.KeyboardIconBinding> defaults =
            LoadKeyboardIconsFromFolder(KeyboardIconRoot);

        for (int i = 0; i < defaults.Count; i++)
        {
            byKey[defaults[i].key] = defaults[i];
        }

        if (savedIcons != null)
        {
            for (int i = 0; i < savedIcons.Count; i++)
            {
                BattleLaneInputRouter.KeyboardIconBinding saved = savedIcons[i];

                if (saved == null ||
                    saved.key == Key.None ||
                    saved.sprite == null)
                {
                    continue;
                }

                byKey[saved.key] = saved;
            }
        }

        return byKey.Values
            .OrderBy(item => (int)item.key)
            .ToList();
    }

    static List<BattleLaneInputRouter.GamepadIconBinding>
        BuildGamepadIconBindings(
            List<BattleLaneInputRouter.GamepadIconBinding> savedIcons)
    {
        var byControl = new Dictionary<
            BattleGamepadControl,
            BattleLaneInputRouter.GamepadIconBinding>();

        string[] guids = AssetDatabase.FindAssets(
            "t:Sprite",
            new[] { GamepadIconRoot });

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            string name = Path.GetFileNameWithoutExtension(assetPath);

            if (!Enum.TryParse(name, out BattleGamepadControl control) ||
                control == BattleGamepadControl.None)
            {
                continue;
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            if (sprite == null)
            {
                continue;
            }

            byControl[control] =
                new BattleLaneInputRouter.GamepadIconBinding
                {
                    control = control,
                    sprite = sprite
                };
        }

        if (savedIcons != null)
        {
            for (int i = 0; i < savedIcons.Count; i++)
            {
                BattleLaneInputRouter.GamepadIconBinding saved = savedIcons[i];

                if (saved == null ||
                    saved.control == BattleGamepadControl.None ||
                    saved.sprite == null)
                {
                    continue;
                }

                byControl[saved.control] = saved;
            }
        }

        return byControl.Values
            .OrderBy(item => (int)item.control)
            .ToList();
    }

    static List<BattleLaneInputRouter.KeyboardIconBinding>
        LoadKeyboardIconsFromFolder(string folder)
    {
        var result =
            new List<BattleLaneInputRouter.KeyboardIconBinding>();

        string[] guids = AssetDatabase.FindAssets(
            "t:Sprite",
            new[] { folder });

        var used = new HashSet<Key>();

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            string name = Path.GetFileNameWithoutExtension(assetPath);

            if (!Enum.TryParse(name, out Key key) ||
                key == Key.None ||
                used.Contains(key))
            {
                continue;
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            if (sprite == null)
            {
                continue;
            }

            used.Add(key);
            result.Add(
                new BattleLaneInputRouter.KeyboardIconBinding
                {
                    key = key,
                    sprite = sprite
                });
        }

        result.Sort((left, right) => ((int)left.key).CompareTo((int)right.key));
        return result;
    }

    static List<BattleLaneInputRouter.LaneBinding>
        BuildLaneBindings(
            BMBattleManager manager,
            List<BattleLaneInputRouter.LaneBinding> savedBindings)
    {
        bool hasSavedBindings =
            savedBindings != null && savedBindings.Count == 6;

        List<BattleLaneInputRouter.LaneBinding> bindings =
            hasSavedBindings
                ? savedBindings
                : BattleLaneInputRouter.CreateDefaultBindings();

        if (!hasSavedBindings)
        {
            for (int lane = 0; lane < bindings.Count; lane++)
            {
                Color color = Color.white;

                if (manager.laneButtonModels != null &&
                    lane < manager.laneButtonModels.Count &&
                    manager.laneButtonModels[lane] != null)
                {
                    Image modelImage =
                        manager.laneButtonModels[lane].GetComponent<Image>();

                    if (modelImage != null)
                    {
                        color = modelImage.color;
                    }
                }

                bindings[lane].displayLabel = string.Empty;
                bindings[lane].displayIcon = null;
                bindings[lane].displayColor = color;
            }
        }

        return bindings;
    }

    static void DisableLegacyHardcodedInput(
        BattleManager manager)
    {
        SerializedObject serializedManager =
            new SerializedObject(manager);

        SerializedProperty keyboardFallback =
            serializedManager.FindProperty(
                "enableKeyboardLaneFallback");

        if (keyboardFallback != null)
        {
            keyboardFallback.boolValue = false;
        }

        SerializedProperty switchKey =
            serializedManager.FindProperty(
                "inputDisplayModeSwitchKey");

        if (switchKey != null)
        {
            switchKey.intValue = (int)Key.None;
        }

        SerializedProperty displayMode =
            serializedManager.FindProperty(
                "inputDisplayMode");

        if (displayMode != null)
        {
            displayMode.enumValueIndex = 0;
        }

        serializedManager.ApplyModifiedPropertiesWithoutUndo();
    }

    static void DeleteExistingEnhancementRoot(
        RectTransform panel)
    {
        Transform existing =
            panel.Find("EnhancedRhythmSystems");

        if (existing != null)
        {
            Object.DestroyImmediate(
                existing.gameObject);
        }
    }

    static RectTransform FindNamedRect(
        string objectName)
    {
        RectTransform[] allRects =
            Object.FindObjectsOfType<
                RectTransform>(true);

        for (int i = 0;
             i < allRects.Length;
             i++)
        {
            RectTransform rect = allRects[i];

            if (rect != null &&
                rect.gameObject.scene.IsValid() &&
                rect.name == objectName)
            {
                return rect;
            }
        }

        return null;
    }

    static void EnsureFolder(
        string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent =
            Path.GetDirectoryName(path)
                ?.Replace("\\", "/");

        string name =
            Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent) &&
            !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(
            parent,
            name);
    }
}
#endif
