# Battle Scene Integration - Step 3 Report

## Obiettivo

Avviare il passaggio dalla timeline prototype alla chart rhythm importata, usando `ChartDataAsset` come sorgente dei pulsanti nella scena reale `Assets/Scenes/Battle.unity`.

## Stato precedente

Lo step 2 aveva gia verificato che:

- la scena `Battle` usa la UI gia montata
- i pulsanti partono nella scena corretta
- input tastiera `A S D / J K L` funziona
- feedback `READY / HIT / MISS` e visibile

Pero i pulsanti arrivavano ancora dalla timeline prototype:

- `Resources/BattleSystem/Beat GuardiansOfTheLight`

## Cosa e stato implementato

### 1. Runner chart runtime

Nuovo file:

- `Assets/Scripts/New Battle System/BattleSystem/BattleRhythmChartRunner.cs`

Il runner legge un `ChartDataAsset` e costruisce una schedule di pulsanti partendo da:

- `chart.rows`
- `row.time`
- `row.lanes[0..5]`

Per ogni lane non vuota viene creato un pulsante nella cella corrispondente.

### 2. Mapping lane -> cella Battle

Il mapping usato e:

```text
lane 0 -> Cell1
lane 1 -> Cell2
lane 2 -> Cell3
lane 3 -> Cell4
lane 4 -> Cell5
lane 5 -> Cell6
```

Questo mantiene la disposizione richiesta:

```text
0 | 1 | 2
---------
3 | 4 | 5
```

### 3. Timing di spawn

Il runner spawna il pulsante prima dell'hit time della chart:

```text
spawnTime = row.time - approachDurationSeconds - 0.1
```

Il `0.1` compensa il piccolo delay interno di `BattleButton`.

Il valore `approachDurationSeconds` viene configurato dal bootstrap usando `timeToReachBar`.

### 4. Spawn diretto da BMBattleManager

`BMBattleManager` ora espone un overload:

```csharp
CreateButton(BMButtonPrefab.Cell cell, Keys key, Sprite sprite, Color color)
```

Questo permette di creare pulsanti sia da:

- timeline prototype, tramite `BMButtonPrefab`
- chart runtime, tramite `BattleRhythmChartRunner`

La vecchia API `CreateButton(BMButtonPrefab prefab)` resta compatibile.

### 5. Bootstrap aggiornato

`BattleSceneRuntimeBootstrap` ora crea anche:

- `BattleRhythmChartRunner`

Di default:

- usa `ChartDataAsset`
- non avvia la timeline prototype per evitare doppioni
- mantiene la timeline come fallback configurabile

Campi aggiunti:

- `useChartDataAsset`
- `playTimelineFallback`
- `maxChartNotesToSpawn`

### 6. Chart default usata in editor

Se nessuna chart e assegnata manualmente, in editor il runner carica:

```text
Assets/RhythmCombat/Generated/Charts/Tutorial Battaglia   Epic Metal Feels_pump-halfdouble_Beginner.asset
```

Questo consente di testare subito la scena `Battle` senza configurazioni manuali.

## File creati

- `Assets/Scripts/New Battle System/BattleSystem/BattleRhythmChartRunner.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleRhythmChartRunner.cs.meta`
- `Docs/BattleSceneIntegration_Step3_Report.md`

## File modificati

- `Assets/Scripts/New Battle System/BattleMaking/BMBattleManager.cs`
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
5. In Console verificare log simili a:

```text
BattleRhythmChartRunner: lette X note da ChartDataAsset ...
BattleRhythmChartRunner: avvio chart ...
BattleRhythmChartRunner: spawn row ... lane ...
```

6. Verificare che i pulsanti partano dalla chart e siano ancora prendibili con:

```text
A | S | D
J | K | L
```

## Nota importante

Questo step collega la chart alla UI Battle, ma non ha ancora sostituito il vecchio criterio di catch con `SpatialJudgmentService`.

Il prossimo step tecnico e:

- usare `NoteMotionService` / `SpatialJudgmentService`
- calcolare Perfect / Good / Bad / Miss dalla distanza dal centro lane
- far dipendere score, damage e feedback dal judgment spaziale
