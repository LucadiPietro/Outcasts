# Battle Scene Integration - Step 5 Report

## Obiettivo

Allineare il movimento visuale delle note nella scena `Assets/Scenes/Battle.unity` con il layer spaziale gia implementato.

Prima di questo step il judgment usava `NoteMotionService`, ma il pulsante visuale continuava a muoversi con una tween verticale:

```text
colonna lane -> barra
```

Con questo step, le note chart-based possono muoversi secondo il cambiamento richiesto:

```text
centro scena -> centro lane al tempo chart -> oltre la lane se non premute
```

## Cosa e stato implementato

### 1. Movimento spaziale opzionale su BattleButton

`BattleButton` ora supporta una modalita visuale spaziale configurabile tramite:

```csharp
ConfigureSpatialMotion(...)
```

La modalita usa:

- `NoteMotionService`
- tempo corrente della chart
- posizione UI di spawn
- posizione UI del centro lane

Il vecchio movimento con `DOMoveY` resta come fallback per compatibilita con timeline/prototype.

### 2. Uso effettivo di NoteMotionService per il visual

Ogni frame, il pulsante chart-based chiede al core:

```csharp
spatialMotionService.GetPosition(spatialNote, currentChartTime)
```

Il `NormalizedProgress` restituito dal core viene usato per interpolare la posizione UI:

```text
progress 0 = centro scena
progress 1 = centro lane
progress > 1 = oltre la lane
```

Questo mantiene coerenti:

- movimento visuale
- tempo di hit
- judgment spaziale

### 3. Spawn sotto root comune della UI

Per permettere il movimento diagonale dal centro verso le sei lane, i pulsanti chart-based non vengono piu istanziati come figli diretti della cella.

Vengono invece istanziati sotto il parent comune delle celle (`BattleRuntime`), cosi possono attraversare liberamente lo spazio UI.

La posizione target della lane viene calcolata usando:

- X della cella (`Cell1` ... `Cell6`)
- Y della hit bar (`UpBar` o `DownBar`)

### 4. Toggle per attivare/disattivare spatial motion

`BMBattleManager` espone:

```csharp
enableSpatialMotion
```

`BattleSceneRuntimeBootstrap` lo configura con:

```csharp
enableSpatialMotion = true
```

Questo permette di disattivare rapidamente il nuovo movimento senza toccare il sistema di judgment.

### 5. Miss centralizzato

La gestione del miss e stata spostata in:

```csharp
BattleManager.ResolveMissedButton(BattleButton battleButton)
```

Ora la stessa logica viene usata sia quando:

- la nota entra nella zona di uscita `UpBarUnsub` / `DownBarUnsub`
- la nota supera il tempo massimo di despawn calcolato dal `NoteMotionService`

Il comportamento resta:

- reset counter
- feedback `MISS CellX`
- danno al party per note defense mancate
- unsubscribe della nota
- fade out visuale

## File modificati

- `Assets/Scripts/New Battle System/BattleSystem/BattleButton.cs`
- `Assets/Scripts/New Battle System/BattleMaking/BMBattleManager.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleManager.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleBarToUnsubcribe.cs`
- `Assets/Scripts/New Battle System/BattleSystem/BattleSceneRuntimeBootstrap.cs`

## Come testare in Unity

1. Aprire `Assets/Scenes/Battle.unity`.
2. Fermare Play Mode se e gia attivo.
3. Attendere la ricompilazione degli script.
4. Premere Play.
5. Verificare che le note chart-based non partano piu gia dalla colonna lane, ma dal centro della scena.
6. Verificare che arrivino al centro della lane al momento utile.
7. Lasciare passare una nota senza premere: deve continuare oltre la lane e poi dare `MISS`.
8. Premere con:

```text
A | S | D
J | K | L
```

9. Verificare che il feedback continui a mostrare:

```text
PERFECT
GOOD
BAD
MISS
```

## Nota

Questo step rende coerente il movimento visuale con il judgment spaziale.

Restano ancora possibili rifiniture successive:

- tuning preciso delle posizioni UI in base alla scena finale
- visual dedicato per hold note
- collegamento del feedback alla UI definitiva invece che al testo debug runtime
