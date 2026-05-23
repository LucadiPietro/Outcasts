# Battle Scene Integration - Step 1 Report

Data: 2026-05-23

## Obiettivo

Verificare quale scena usare come base per integrare il layer rhythm-combat sviluppato finora dentro la battle reale gia' montata dal senior.

Questo step non modifica ancora il gameplay: serve a decidere la scena target corretta e a capire cosa e' gia' presente.

## Conclusione

La scena target per l'integrazione deve essere:

`Assets/Scenes/Battle.unity`

Motivi:

- e' la scena chiamata `Battle` indicata dal senior;
- e' l'unica scena Battle abilitata in `ProjectSettings/EditorBuildSettings.asset`;
- contiene gia' la UI principale della battle;
- deve diventare il punto di ingresso reale del combat system.

La scena:

`Assets/Scenes/Others/Battle.unity`

non va usata come scena finale, ma e' molto utile come riferimento tecnico per recuperare/integrare:

- `BattleManager`
- `BattleUIManager`
- `BattleInputManager`
- `BMBattleManager`
- `CellDivisor`
- `PlayableDirector`
- celle `Cell1` ... `Cell6`
- logica di spawn dei `BattleButton`

## Evidenze

### Build Settings

In `ProjectSettings/EditorBuildSettings.asset` risulta abilitata questa scena:

`Assets/Scenes/Battle.unity`

La scena `Assets/Scenes/Others/Battle.unity` non e' presente tra le scene abilitate.

### Assets/Scenes/Battle.unity

La scena principale contiene gia' UI e struttura visiva:

- `Canvas`
- `EventSystem`
- `Main Camera`
- gruppi UI `Players` ed `Enemies`
- barre health/super
- linee visuali tipo `LineUp`, `LineCenter`, `LineDown`
- un oggetto `Button`

Pero' non risultano agganciati nella scena principale:

- `BattleManager`
- `BattleUIManager`
- `BattleInputManager`
- `CellDivisor`
- `BMBattleManager`
- timeline/playable per generare i pulsanti battle

Quindi, allo stato attuale, `Assets/Scenes/Battle.unity` ha la UI montata ma non ha ancora la parte runtime che fa partire i pulsanti di battaglia.

### Assets/Scenes/Others/Battle.unity

La scena sotto `Others` contiene invece la parte di gameplay/prototipo:

- GameObject `Manager`
- `BattleManager`
- `BattleUIManager`
- `BattleInputManager`
- `BMBattleManager`
- `CellDivisor`
- riferimenti a player/enemy
- `AudioController` con `PlayableDirector`
- celle `Cell1` ... `Cell6`
- barre `UpBar`, `DownBar`, `UpBarUnsub`, `DownBarUnsub`

Questa scena dimostra che esiste gia' una pipeline battle basata su:

`Timeline / PlayableDirector -> BMButtonPrefab -> BMBattleManager -> BattleButton -> BattleManager`

## Impatto sul lavoro fatto finora

Il layer `RhythmCombat` gia' implementato e' valido come core separato:

- geometria delle lane;
- movimento centro-lane;
- judgment spaziale;
- input configurabile;
- test visuale isolato.

Pero' non e' ancora collegato alla scena reale `Battle.unity`.

`SpatialChartVisualTest` resta utile come debug harness, ma non soddisfa da solo la richiesta attuale del senior, perche' gira fuori dalla scena Battle reale.

## Decisione tecnica

Usare `Assets/Scenes/Battle.unity` come scena finale di integrazione.

Usare `Assets/Scenes/Others/Battle.unity` come sorgente di riferimento per capire come:

- vengono spawnati i pulsanti;
- vengono associati alle celle;
- vengono iscritti al manager;
- viene calcolato input/hit;
- vengono applicati danni a player/enemy.

Non conviene spostare direttamente tutta la scena `Others/Battle.unity` sopra `Battle.unity` senza controllo, perche' rischieremmo di sovrascrivere la UI gia' montata dal senior.

## Prossimo step consigliato

Step 2: integrare nella scena `Assets/Scenes/Battle.unity` i manager runtime mancanti, mantenendo la UI gia' presente.

In pratica:

1. aggiungere/collegare un GameObject `BattleRuntime` o equivalente;
2. portare in `Battle.unity` i riferimenti necessari a `BattleManager`, `BattleUIManager`, `BattleInputManager`, `BMBattleManager` e `CellDivisor`;
3. collegare le sei lane/celle della UI esistente ai sei indici lane `0..5`;
4. verificare che, premendo Play, i pulsanti vengano istanziati nella scena reale;
5. solo dopo collegare il layer `RhythmCombat` al movimento/judgment definitivo.

## Nota per il senior

La UI della battle risulta gia' montata nella scena principale `Battle.unity`, ma la parte runtime dei pulsanti non e' ancora agganciata li'. La scena `Others/Battle.unity` contiene invece una pipeline gameplay funzionante/prototipale. Il prossimo lavoro e' integrare quella pipeline, e poi il nuovo layer rhythm-combat, dentro `Battle.unity` senza ricostruire o sovrascrivere la UI gia' preparata.
