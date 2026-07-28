# Battle Scene Integration - Step 4 Report

## Obiettivo

Integrare il judgment spaziale nella scena reale `Assets/Scenes/Battle.unity`, usando il layer core gia implementato:

- `SpatialJudgmentService`
- `NoteMotionService`
- `BattlefieldLayout`
- `SpatialJudgmentConfig`
- `JudgmentResult`

Lo scopo e passare da un semplice `HIT/MISS` basato sulla trigger bar a un giudizio:

- `PERFECT`
- `GOOD`
- `BAD`
- `MISS`

## Cosa e stato implementato

### 1. Metadati spaziali sui pulsanti

`BattleButton` ora puo contenere i dati necessari al judgment spaziale:

- `laneIndex`
- `hitTimeSeconds`
- `spatialNote`
- `hasSpatialJudgmentData`

Quando il pulsante nasce dalla chart, riceve questi dati tramite:

```csharp
ConfigureSpatialJudgment(int laneIndexValue, double hitTimeSecondsValue)
```

I pulsanti vecchi/prototype restano compatibili, perche se non hanno questi dati usano ancora il comportamento precedente.

### 2. Chart runner collegato al clock della chart

`BattleRhythmChartRunner` ora aggiorna il tempo corrente della chart dentro `BattleManager`:

```csharp
BattleManager.Instance.SetChartTimeSeconds(elapsedSeconds);
```

Quando spawna un pulsante, passa anche:

- lane della chart
- hit time della chart

Questo permette a `SpatialJudgmentService` di valutare la distanza dal centro lane al momento dell'input.

### 3. BMBattleManager supporta spawn con dati spaziali

`BMBattleManager` ora ha un overload di spawn:

```csharp
CreateButton(
    BMButtonPrefab.Cell cell,
    Keys key,
    Sprite sprite,
    Color color,
    int laneIndex,
    double hitTimeSeconds)
```

La timeline prototype continua a usare il metodo precedente.

### 4. BattleManager usa SpatialJudgmentService

`BattleManager` ora costruisce un servizio spaziale configurabile:

- lane spacing
- row offset
- tolerance radius
- perfect percent
- good percent
- bad percent
- despawn delay

Il bootstrap configura il servizio usando `timeToReachBar` come durata di approach.

Quando premi un tasto:

1. recupera il pulsante nella finestra
2. se il pulsante ha dati spaziali, valuta con `SpatialJudgmentService`
3. mostra il grade reale
4. applica danno con moltiplicatore in base al grade

### 5. Allineamento hit bar / judgment spaziale

Il target visuale dei pulsanti e stato allineato alle barre `UpBar` / `DownBar`, cioe al centro utile della lane.

Le barre `UpBarUnsub` / `DownBarUnsub` restano come zona di uscita/miss, ma non sono piu usate come target principale del movimento.

Questo e importante perche il judgment spaziale considera `hitTimeSeconds` come il momento in cui la nota arriva al centro lane.

### 6. Feedback visuale aggiornato

La UI ora puo mostrare:

```text
PERFECT CellX
GOOD CellX
BAD CellX
MISS CellX
```

I colori sono:

- Perfect: ciano/verde
- Good: verde
- Bad: arancio
- Miss: rosso

### 7. Moltiplicatore danno per judgment

Per le note attack:

```text
Perfect -> 1.00x
Good    -> 0.75x
Bad     -> 0.40x
Miss    -> 0.00x
```

Il comportamento defense resta coerente col sistema precedente:

- se prendi la nota defense, eviti il danno
- se la perdi, `BattleBarToUnsubcribe` chiama `DefenceRoutine`

### 8. Note risolte e zona di uscita

`BattleButton` ora espone uno stato `isResolved`.

Quando una nota viene presa, `KillButton()` marca la nota come risolta e disabilita il collider. In questo modo la zona `UpBarUnsub` / `DownBarUnsub` non puo piu sovrascrivere un `PERFECT`, `GOOD` o `BAD` con un falso `MISS`.

Quando invece una nota arriva davvero alla zona di uscita senza essere presa, `BattleBarToUnsubcribe` la marca come risolta e mostra `MISS`.

## File modificati

- `Assets/Scripts/New Battle System/BattleSystem/BattleButton.cs`
- `Assets/Scripts/New Battle System/BattleMaking/BMBattleManager.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleRhythmChartRunner.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleManager.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleSceneRuntimeBootstrap.cs`

## Verifica eseguita

Comando:

```powershell
dotnet build Outcasts.sln -m:1
```

Risultato:

- errori: `0`
- warning: solo warning gia presenti nel progetto

## Come testare in Unity

1. Aprire `Assets/Scenes/Battle.unity`.
2. Fermare Play Mode se e gia attivo.
3. Attendere la ricompilazione script.
4. Premere Play.
5. Prendere le note con:

```text
A | S | D
J | K | L
```

6. Verificare che il feedback non sia piu solo `HIT`, ma uno tra:

```text
PERFECT
GOOD
BAD
MISS
```

7. In Console verificare log simili a:

```text
BattleManager: Perfect su Cell1 lane=0 delta=0.012 input=Cell1. Counter: 1
```

## Nota

Questo step integra il judgment spaziale nella scena `Battle`, ma resta ancora possibile rifinire:

- valori di tolleranza da inspector
- tuning dei moltiplicatori di danno
- visualizzazione piu elegante del grade nella UI definitiva
