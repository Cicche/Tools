# Tools_2.0

Software desktop WPF per gestione operativa di macchine in rete LAN:
- caricamento macchine da XML
- gestione credenziali cifrate
- operazioni batch (Ping, Riavvio, Shutdown, Kill, Copy, Delete remoto)
- accesso remoto (File Explorer, Desktop Remoto)
- scanner IP integrato

## Requisiti
- Windows
- .NET Framework 4.7.2
- Permessi rete adeguati sui target (share admin/WMI dove richiesto)

## Struttura progetto
- `Tools.Wpf/Tools_2.0.csproj`: applicazione WPF (UI)
- `Tools.csproj`: libreria Core (dominio/servizi/repository)
- `Tool_List.xml`: file macchine (creato automaticamente se assente)
- `Tool_Credentials.bin`: store credenziali cifrato
- `Logs/tools-YYYYMMDD.log`: log applicativo giornaliero

## Avvio
- Eseguibile Release: `Tools.Wpf/bin/Release/net472/Tools_2.0.exe`
- All'avvio l'app tenta di leggere `Tool_List.xml` nella cartella dell'eseguibile.
- Se l'XML non esiste o e vuoto:
- viene creato un template di esempio
- il file viene ricaricato automaticamente senza riavviare l'app

## Formato XML
Formato moderno supportato:
- root: `ToolsConfig`
- sezioni: `ArrayOfCategory` e `ArrayOfPC`

Categorie standard:
- `CMP`, `TRD`, `DOK`, `SERVER`, `GW`, `MFC`, `OBTS`

Ogni macchina usa:
- `Type`
- `Nome`
- `Ip`
- `User` e `Password` sono previsti ma vengono sanitizzati (vuoti) dopo migrazione/gestione sicura

Compatibilita legacy:
- Se viene rilevato un XML legacy, l'app lo gestisce in modo trasparente e migra al formato moderno.

## Credenziali e sicurezza
Le credenziali non sono salvate in chiaro nel file XML operativo.

Store credenziali:
- file: `Tool_Credentials.bin`
- cifratura: DPAPI Windows `DataProtectionScope.CurrentUser`

Implicazioni pratiche:
- il file copiato su un altro PC/altro utente normalmente non e decifrabile
- se un attaccante compromette lo stesso account Windows, puo potenzialmente leggere lo store

Configurazione strict mode:
- file: `Tools.Wpf/App.config`
- chiave: `Credentials.StrictMode`

## Operazioni disponibili
Dal pannello Azioni:
- Ping
- Riavvio
- Shutdown
- Kill
- Refresh UI
- Reset credenziali

Da riga Copy:
- campo `Origine copy` (manuale o selezione cartella tramite pulsante)
- campo `Destinazione copy` manuale
- pulsante `Copy`

Formato destinazione copy consigliato:
- `C$\Example\temp`
- `D$\Deploy\bin`

La destinazione viene risolta per ogni macchina selezionata sul proprio IP:
- esempio: `C$\Example\temp` -> `\\<IP_TARGET>\C$\Example\temp`

Da riga Delete remoto:
- campo `Delete remoto` manuale o valorizzato tramite pulsante cartella
- pulsante `Delete`
- conferma obbligatoria prima dell'esecuzione

Formato delete consigliato:
- `C$\Example\temp`
- `D$\Deploy\old`

Il percorso viene risolto per ogni macchina selezionata sul proprio IP:
- esempio: `C$\Example\temp` -> `\\<IP_TARGET>\C$\Example\temp`

Protezioni delete:
- non accetta path vuoti
- non accetta radici admin share come `C$`
- non accetta path con traversal `..`

## Selezione macchine
- click sinistro su pannello categoria: seleziona tutte le macchine della categoria
- click destro su pannello categoria: deseleziona tutte
- checkbox su singola macchina: selezione puntuale
- bordo verde: macchina selezionata
- colore bottone: stato ultimo risultato operazione

## Menu contestuale macchina
Click destro su bottone macchina:
- File Explorer
- Desktop Remoto
- Edit

In Edit:
- modifica categoria/nome/ip/user/password
- password con conferma
- pulsante rosso di eliminazione con doppia conferma

## Scanner IP
Pannello laterale espandibile:
- range Start/End
- timeout scan
- risultati con IP e host
- doppio click o menu contestuale su riga risultato per aggiungere macchina precompilata

## Timeout e retry
- configurabili nell'header del pannello Azioni
- default timeout: `8000 ms`
- default retry: `3`

Note operative:
- per `Reboot` e `Shutdown` e prevista verifica post-comando (transizione offline)
- per `Reboot/Shutdown/Kill` e presente pre-check di raggiungibilita host

## Build
Debug:
```powershell
dotnet build Tools.Wpf/Tools_2.0.csproj -c Debug
```

Release:
```powershell
dotnet build Tools.Wpf/Tools_2.0.csproj -c Release
```

## Troubleshooting rapido
- "Credenziali non disponibili":
- impostare credenziali macchina o categoria dall'app

- "Connessione share fallita":
- verificare permessi, firewall, policy admin share, credenziali

- "Host non raggiungibile":
- verificare IP, routing/VLAN, firewall ICMP

- XML non caricato:
- controllare sintassi file e presenza nodo `ArrayOfPC`

## Note finali
Questo progetto e pensato per ambienti operativi interni.  
Prima dell'uso in produzione estesa, consigliato:
- backup regolare di XML e cartella `Logs`
- revisione permessi account usati per operazioni remote
- test in rete reale su gruppi macchina campione
