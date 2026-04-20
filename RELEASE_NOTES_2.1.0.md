# Tools 2.1.0 - Release Notes

Data release: 2026-04-17

## 1) Fix `KILL` remoto (WMI)
- Corretto il path di terminazione processi via WMI su host che rispondevano con `Parametro non valido`.
- La chiamata `Terminate` ora usa i `MethodParameters` corretti e imposta `Reason=0` quando disponibile.
- Migliorata la gestione del `ReturnValue` WMI e la mappatura errori.
- Impatto utente: riduzione dei falsi KO su `Kill` con target raggiungibili e credenziali valide.

## 2) Layout finestra: log + scanner IP
- Il pannello `Log` ora termina alla colonna principale (non occupa la colonna scanner).
- Lo `Scanner IP` quando aperto occupa anche la porzione destra del log (stessa colonna dedicata).
- Effetto visivo coerente con la richiesta: area scanner separata e non “bucata” dal log.

## 3) Scanner IP: gestione lista risultati
- Abilitato scroll verticale esplicito sulla lista risultati scanner (`DataGrid`) quando i risultati superano lo spazio visibile.
- Migliorata usabilità in reti con molti host rilevati.

## Extra UX log
- Doppio click sinistro nel riquadro log: apre il file log del giorno corrente.
- Click destro nel riquadro log: apre il file log (fallback all'ultimo disponibile se quello del giorno non esiste).

## Versionamento applicato
- `Version`: `2.1.0`
- `AssemblyVersion`: `2.1.0.0`
- `FileVersion`: `2.1.0.0`
- `InformationalVersion`: `2.1.0`

