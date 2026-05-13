# Tools - Release History

Questo file contiene lo storico progressivo delle release, con la versione piu recente in alto.

## 2.6.0 - 2026-05-13

### UI e layout
- Ridotta la larghezza minima reale della finestra quando i pannelli categoria contengono una sola colonna di macchine.
- I pannelli categoria usano ora una larghezza minima derivata dal contenuto, evitando spazio vuoto inutile quando possibile.
- Il pannello `Copy` termina subito dopo il pulsante `Copy`; aggiunto un pannello separato `Delete remoto` sulla stessa riga.
- Bordo verde di selezione macchina ridotto da 6 a 4 per maggiore leggibilita senza invadere troppo il bottone.

### Delete remoto
- Aggiunta operazione batch `Delete` sulle macchine selezionate.
- Il path viene espresso in formato admin share, ad esempio `C$\Example\temp`.
- Il comando risolve il path per ogni target come `\\<IP_TARGET>\C$\Example\temp`.
- Aggiunta conferma obbligatoria in stile `Reboot/Shutdown`, con elenco macchine e percorso da cancellare.
- Aggiunte protezioni base: blocco path vuoti, radici admin share tipo `C$` e path con traversal `..`.
- L'operazione usa pre-ping, credenziali cifrate e connessione SMB temporanea come la Copy.

### Release package
- `README.md` e `RELEASE.md` vengono preparati anche in `Tools.Wpf\bin\Release\net472`, insieme all'eseguibile Release.

### Versionamento
- `Version`: `2.6.0`
- `AssemblyVersion`: `2.6.0.0`
- `FileVersion`: `2.6.0.0`
- `InformationalVersion`: `2.6.0`

## 2.5.0 - 2026-05-08

### Migliorie hostname scanner/ping
- Verifica hostname migliorata per ridurre falsi mismatch dovuti a DNS/PTR locali non coerenti.
- Confronto ora eseguito su:
  - nome DNS normalizzato (senza dominio/workgroup),
  - fallback nome NetBIOS (`nbtstat -A`) normalizzato.
- Nei log mismatch vengono mostrati separatamente `dns=` e `netbios=` per diagnosi piu chiara.

### Categoria OBTS a scomparsa
- Pannello `OBTS` reso dinamico con layout orizzontale prima del ritorno a capo (sfrutta la larghezza disponibile, poi scende di riga).
- Ridotta la probabilita di taglio degli elementi quando aumentano le macchine in categoria.

### Password: modalita visibile
- Aggiunta opzione `Mostra` nei form:
  - Aggiunta macchina,
  - Edit macchina,
  - Credenziali categoria.
- Password e conferma restano sincronizzate sia in vista nascosta sia in vista testuale.

### Lista risultati IP scanner
- Migliorata evidenziazione riga in hover/selezione per rendere piu chiaro quale macchina si sta per aggiungere.
- Contrasto testo mantenuto alto anche su riga selezionata.

### Versionamento
- `Version`: `2.5.0`
- `AssemblyVersion`: `2.5.0.0`
- `FileVersion`: `2.5.0.0`
- `InformationalVersion`: `2.5.0`

## 2.4.0 - 2026-04-23

### UI e usabilita
- Riposizionato il toggle `OBTS` nell'area categorie (agganciato visivamente ai pannelli inferiori), invece che sul lato destro vicino alla legenda.
- Migliorata la leggibilita delle frecce toggle (`Scanner` e `OBTS`):
  - pulsanti con padding azzerato;
  - font piu visibile e centratura contenuto.
- Comportamento coerente con il layout dinamico dei pannelli categoria.

### Versionamento
- `Version`: `2.4.0`
- `AssemblyVersion`: `2.4.0.0`
- `FileVersion`: `2.4.0.0`
- `InformationalVersion`: `2.4.0`

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
