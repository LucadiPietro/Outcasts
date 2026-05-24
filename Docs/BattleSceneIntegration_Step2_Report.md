# Battle Scene Integration - Step 2 Report

## Obiettivo

Collegare la scena reale `Assets/Scenes/Battle.unity` alla parte runtime del `New Battle System`, senza sovrascrivere la UI gia montata e senza usare la scena prototype `Assets/Scenes/Others/Battle.unity` come scena finale.

## Cosa e stato implementato

### 1. Bootstrap runtime dedicato alla scena Battle

Nuovo file:

- `Assets/Scripts/New Battle System/BattleSystem/BattleSceneRuntimeBootstrap.cs`

Il bootstrap si attiva automaticamente solo quando Unity carica:

- `Assets/Scenes/Battle.unity`

Non interviene su:

- `Assets/Scenes/Others/Battle.unity`

Questo evita di duplicare o sporcare la scena prototype.

### 2. Collegamento automatico dei manager runtime

Quando la scena `Battle` parte in Play Mode, il bootstrap crea e configura:

- `BattleInputManager`
- `BattleUIManager`
- `BattleManager`
- `BMBattleManager`
- `CellDivisor`
- `AudioController` con `PlayableDirector`

Il `PlayableDirector` carica la timeline esistente:

- `Resources/BattleSystem/Beat GuardiansOfTheLight`

La timeline viene quindi usata per far partire i pulsanti gia definiti dal sistema `BMButtonPrefab` / `BMBattleManager`.

### 3. Celle e barre runtime sopra la UI esistente

La UI della scena `Battle` non e stata modificata o ricostruita.

Il bootstrap crea sotto il `Canvas` un root runtime chiamato:

- `BattleRuntime`

Dentro crea:

- `Cell1`
- `Cell2`
- `Cell3`
- `Cell4`
- `Cell5`
- `Cell6`
- `UpBar`
- `DownBar`
- `UpBarUnsub`
- `DownBarUnsub`

Le celle sono disposte in tre colonne e partono dalla fascia centrale della UI. Le barre `UpBar` e `DownBar` sono allineate alle linee gia presenti in scena (`LineUp` e `LineDown`) e servono al sistema esistente per iscrivere i pulsanti quando entrano nella zona utile.

### 4. Template runtime per i pulsanti

Il bootstrap crea un template runtime invisibile:

- `RuntimeBattleButtonTemplate`

Questo template contiene:

- `RectTransform`
- `Image`
- `BattleButton`
- `BoxCollider2D`
- `Rigidbody2D`

Il `BMBattleManager` lo istanzia quando la timeline genera un `BMButtonPrefab`.

### 5. Protezioni minime sui componenti esistenti

Sono state aggiunte modifiche difensive leggere:

- `BMBattleManager` riattiva le istanze create da template inattivo.
- `BattleUIManager` evita `NullReferenceException` se manca il testo counter.
- `BattleManager` verifica che `AudioController` e `PlayableDirector` siano presenti prima di chiamare `Play`.
- `BattleManager` evita crash se durante un test mancano player/enemy validi.
- `BattleBarToUnsubcribe` ignora collisioni che non arrivano da `BattleButton`.

Queste modifiche non cambiano il comportamento valido del sistema, ma rendono la scena `Battle` testabile senza dover ricostruire manualmente tutti i riferimenti della prototype.

## File creati

- `Assets/Scripts/New Battle System/BattleSystem/BattleSceneRuntimeBootstrap.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleSceneRuntimeBootstrap.cs.meta`

## File modificati

- `Assets/Scripts/New Battle System/BattleMaking/BMBattleManager.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleManager.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleUIManager.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleBarToUnsubcribe.cs`

## Verifica eseguita

Comando:

```powershell
dotnet build Outcasts.sln -m:1
```

Risultato:

- build completata
- errori: `0`
- warning: presenti warning gia noti del progetto, incluso `MonoScriptInfoGenerator` e warning su campi non assegnati

## Come testare in Unity

1. Aprire `Assets/Scenes/Battle.unity`.
2. Premere Play.
3. Verificare in Hierarchy la creazione automatica di:
   - `Battle Scene Runtime Bootstrap`
   - `BattleRuntime`
   - `AudioController`
4. Dopo circa `1.5` secondi, il `PlayableDirector` deve avviare la timeline `Beat GuardiansOfTheLight`.
5. I pulsanti generati dalla timeline devono comparire nelle celle runtime e muoversi verso le barre della UI.
6. In Console deve apparire:

```text
BattleSceneRuntimeBootstrap: Battle scene collegata a BattleManager, BMBattleManager, celle, barre e timeline.
```

## Assunzioni

- La scena finale da usare e `Assets/Scenes/Battle.unity`, non `Assets/Scenes/Others/Battle.unity`.
- La timeline `Beat GuardiansOfTheLight` rimane la sorgente attuale dei pulsanti del prototype.
- In questo step non e stato ancora collegato il nuovo core `RhythmCombat` con chart `.sm` alla UI di battaglia. Questo step serve a far partire la parte battle/timeline dentro la scena `Battle` reale.

## Prossimo step consigliato

Verificare in Play Mode che i pulsanti partano visivamente nella scena `Battle`. Se il movimento e confermato, lo step successivo e sostituire o affiancare la sorgente timeline con la sorgente chart `.sm` / `CombatRhythmEngine`, mantenendo la stessa UI gia montata.
