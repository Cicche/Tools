# Tools 2.2.0 - Release Notes

Data release: 2026-04-20

## Panoramica
Questa release chiude le azioni P0, P1, P2 e P3 emerse dalla revisione tecnica post-2.1.0, con focus su sicurezza credenziali, robustezza batch, gestione share remoti e usabilita scanner/log.

## P0 - Strict mode realmente bloccante
- Corretto il bootstrap credenziali: in `STRICT` non viene piu scritto nulla nello store cifrato quando l'XML contiene credenziali legacy (macchina o categoria).
- La verifica `STRICT` ora avviene prima di `SetCategoryCredentials(...)`.
- Effetto pratico: niente persistenza involontaria di credenziali da XML in modalita strict.

## P1 - Timeout e comportamento operazioni remote
- Rimosso il bypass automatico di `STRICT` su XML legacy.
- Migliorata la policy timeout batch:
  - al timeout viene segnalato che l'operazione potrebbe completarsi comunque lato target;
  - viene tracciato il completamento tardivo (successo/errore) per ridurre ambiguita diagnostica.
- Effetto pratico: log piu affidabile su comandi lenti/non annullabili.

## P1 - Sessioni share remote
- Aggiunta disconnessione best-effort della share a fine copia (`WNetCancelConnection2`) per ridurre sessioni SMB residue.
- Effetto pratico: minori conflitti credenziali nelle operazioni successive.

## P2 - Ottimizzazione Copy batch
- Rimossa l'apertura automatica di Explorer dopo `CopyFolder` (prima avveniva per ogni macchina).
- Effetto pratico: batch copy piu pulito e leggero lato UI.

## P2 - Scanner IP (DNS)
- Aggiunto timeout alla risoluzione DNS host per ogni IP trovato attivo.
- Effetto pratico: scansione piu reattiva su reti con reverse-DNS lento/non affidabile.

## P3 - Apertura file log
- Apertura log da UI ora punta direttamente al file log piu recente disponibile nella cartella `Logs`.
- Effetto pratico: comportamento piu consistente vicino al cambio data.

## Versione
- `Version`: `2.2.0`
- `AssemblyVersion`: `2.2.0.0`
- `FileVersion`: `2.2.0.0`
- `InformationalVersion`: `2.2.0`

## Note operative
- Nessuna modifica all'interfaccia utente principale.
- Nessuna build eseguita in questa attivita (come richiesto).

