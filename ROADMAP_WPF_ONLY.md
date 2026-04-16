# Roadmap migrazione WPF only

## Stato decisione
- WinForms e in maintenance/deprecato.
- Nuove feature: solo su `Tools.Wpf`.

## Obiettivi attivi
1. Congelamento WinForms.
2. Core unico riusabile e indipendente dalla UI.
3. Migrazione incrementale a MVVM in WPF.
4. Hardening credenziali e policy operative.
5. Stabilita, retry/timeout/cancel e logging strutturato.
6. Parita funzionale WPF con checklist di verifica.
7. Dismissione WinForms.

## Regole operative
- Non introdurre nuova logica in `Form1`, `Form2`, `Liste`, `Funzioni`.
- Ogni nuova funzione passa da `IToolsCoreFacade`.
- In WPF, spostare progressivamente stato e orchestrazione in ViewModel.

## Criteri di completamento fase
- WPF copre tutti i flussi operativi usati in produzione.
- WinForms non e piu usato come entrypoint operativo.
- Code-behind WPF limitato a eventi UI e wiring finestra.

## Stato attuale
- [x] Punto 1 completato (WinForms deprecato all'avvio).
- [x] Punto 2 avviato/completato lato WPF (facade Core usata come entrypoint logico).
- [x] Punto 3 avviato (MainViewModel introdotto, binding principali su path/copy/timeout/retry/categorie).
- [x] Punto 4 completato (pre-check credenziali su batch/azioni remote).
- [x] Punto 5 completato (log file per batch/operazioni + riduzione popup non necessari).
- [x] Punto 6 avviato (checklist parita in `WPF_PARITY_CHECKLIST.md`).
- [x] Punto 7 completato lato build (project `Tools` convertito a libreria Core, file WinForms esclusi dalla compilazione).
