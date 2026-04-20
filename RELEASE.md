# Tools - Release History

Questo file contiene lo storico progressivo delle release, con la versione piu recente in alto.

## 2.3.0 - 2026-04-20

### Migliorie
- Scanner IP: aggiunto selettore rete vicino al titolo `Scanner IP` per scegliere la subnet da scandire.
- Rilevazione multi-scheda: enumerate tutte le reti IPv4 disponibili (incluse schede virtuali attive), non solo la rete default.
- Aggiornamento automatico range: cambiando rete nel selettore vengono valorizzati `Start` e `End` coerenti con la subnet selezionata.
- Priorita visualizzazione: le reti con gateway IPv4 sono marcate come `[Default]` e proposte in alto.
- Fallback robusto: se nessuna rete valida e disponibile, resta attivo il range `192.168.1.1 - 192.168.1.254`.

### Versionamento
- `Version`: `2.3.0`
- `AssemblyVersion`: `2.3.0.0`
- `FileVersion`: `2.3.0.0`
- `InformationalVersion`: `2.3.0`

## 2.2.0 - 2026-04-20

### Sicurezza e affidabilita
- Strict mode realmente bloccante: impedita la persistenza involontaria di credenziali legacy nel bootstrap.
- Rimosso bypass automatico strict su XML legacy.
- Timeout batch con tracciamento completamenti tardivi per ridurre ambiguita operativa.
- Disconnessione best-effort share SMB dopo copy per ridurre conflitti sessione.

### Performance e usabilita
- Rimosso `Process.Start("explorer", ...)` automatico su copy batch.
- Timeout DNS nello scanner per reti con reverse lookup lento.
- Apertura log da UI resa piu consistente (file piu recente disponibile).

### Versionamento
- `Version`: `2.2.0`
- `AssemblyVersion`: `2.2.0.0`
- `FileVersion`: `2.2.0.0`
- `InformationalVersion`: `2.2.0`

## 2.1.0 - 2026-04-17

### Correzioni funzionali
- Fix `KILL` remoto via WMI (`Terminate` con parametri corretti, `Reason=0`, gestione `ReturnValue`).
- Corretto layout log/scanner: il log non invade la colonna scanner, e scanner esteso verticalmente.
- Lista risultati scanner resa scrollabile.

### UX
- Doppio click sinistro e click destro nel pannello log per aprire il file log.

### Versionamento
- `Version`: `2.1.0`
- `AssemblyVersion`: `2.1.0.0`
- `FileVersion`: `2.1.0.0`
- `InformationalVersion`: `2.1.0`

## 2.0.0 - Baseline WPF

### Milestone
- Dismissione WinForms e consolidamento su WPF.
- Porting dei flussi principali: caricamento XML, pannelli categoria dinamici, operazioni remote batch/singole.
- Introduzione gestione credenziali cifrate (`Tool_Credentials.bin`) e sanitizzazione XML.
- Layout e logging operativo portati in interfaccia WPF.

### Versionamento
- `Version`: `2.0.0`
- `AssemblyVersion`: `2.0.0.0`
- `FileVersion`: `2.0.0.0`
- `InformationalVersion`: `2.0.0`

