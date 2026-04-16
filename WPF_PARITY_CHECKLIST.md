# WPF parity checklist (fase 4-5-6)

## 1) Gestione credenziali (hardening)
- [x] Operazioni remote batch eseguite solo su macchine con credenziali risolte.
- [x] Macchine senza credenziali marcate KO + deselezione automatica.
- [x] Avviso esplicito a bootstrap quando store credenziali e assente.
- [x] Nessun fallback implicito a credenziali vuote.

## 2) Stabilita operativa e osservabilita
- [x] Retry/timeout/cancel uniformi sui batch.
- [x] Logging operativo su file anche da WPF (batch start/end/cancel, operation outcome).
- [x] Warning non bloccanti su input non valido (meno popup, piu log).

## 3) Parita funzionale da verificare
- [ ] Caricamento XML + categorie dinamiche.
- [ ] Selezione macchina singola e categoria (sx tutto / dx nessuno).
- [ ] Ping batch su selezionate.
- [ ] Riavvio/Shutdown/Kill batch su selezionate con credenziali valide.
- [ ] Copy batch su selezionate con credenziali valide.
- [ ] Context menu macchina: File Explorer / Desktop remoto / Elimina.
- [ ] Inserimento macchina in categoria (+).
- [ ] Inserimento credenziali categoria (+).
- [ ] Reimport XML legacy credenziali.
- [ ] Reset store credenziali.
- [ ] Log severita (info/warn/error) in UI + file.

## 4) Test manuale consigliato
1. Avvio con XML valido e store assente: verificare warning esplicito in log.
2. Selezionare macchine miste (con/senza credenziali), lanciare Riavvio:
   atteso = solo quelle con credenziali vengono eseguite, le altre KO immediate.
3. Lanciare Copy senza origine/destinazione:
   atteso = warning in log, nessun popup bloccante.
4. Simulare host offline e lanciare Ping:
   atteso = retry su errori retryable, esito KO coerente, deselezione su KO.
5. Verificare file `Logs/tools-YYYYMMDD.log` con eventi batch e operation.
