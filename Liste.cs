using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tools
{
    internal class Liste
    {


        ArrayList listamacchine = new ArrayList();
        
        ArrayList listaCmp = new ArrayList();
        ArrayList listaTrd = new ArrayList();
        ArrayList listaDok = new ArrayList();
        ArrayList listaSrv = new ArrayList();
        ArrayList listaGw = new ArrayList();
        ArrayList listaMfc = new ArrayList();

        ArrayList listaButton = new ArrayList();

        ArrayList listaCmpButton = new ArrayList();
        ArrayList listaTrdButton = new ArrayList();
        ArrayList listaDokButton = new ArrayList();
        ArrayList listaSrvButton = new ArrayList();
        ArrayList listaGwButton = new ArrayList();
        ArrayList listaMfcButton = new ArrayList();

        ArrayList listaCheck = new ArrayList();

        ArrayList listaCmpCheck = new ArrayList();
        ArrayList listaTrdCheck = new ArrayList();
        ArrayList listaDokCheck = new ArrayList();
        ArrayList listaSrvCheck = new ArrayList();
        ArrayList listaGwCheck = new ArrayList();
        ArrayList listaMfcCheck = new ArrayList();

        public ArrayList getlistamacchine { get =>  listamacchine; }
        public ArrayList getlistaButton { get => listaButton; }
        public ArrayList getlistaCheck { get => listaCheck; }

        public ArrayList getCmpButton { get => listaCmpButton; }
        public ArrayList getTrdButton { get => listaTrdButton; }
        public ArrayList getDokButton { get => listaDokButton; }
        public ArrayList getSrvButton { get => listaSrvButton; }
        public ArrayList getGwButton { get => listaGwButton; }
        public ArrayList getMfcButton { get => listaMfcButton; }

        public ArrayList getCmpCheck { get => listaCmpCheck; }
        public ArrayList getTrdCheck { get => listaTrdCheck; }
        public ArrayList getDokCheck { get => listaDokCheck; }
        public ArrayList getSrvCheck { get => listaSrvCheck; }
        public ArrayList getGwCheck { get => listaGwCheck; }
        public ArrayList getMfcCheck { get => listaMfcCheck; }



        public void loadPC()
        {
           
            Macchine listapc = Macchine.Deserialize("Tool_List.xml");
            if (listapc == null)

            { listapc = Macchine.Deserialize("Tool_List.xml"); }


            if (listapc != null)
            {
                foreach (PC pc in listapc)
                {
                   // listamacchine.Add(pc);

                    switch (pc.Type.ToUpper())
                    {
                        case "CMP":
                            listaCmp.Add(pc);
                            loadBtn(pc, listaCmp.Count, listaCmpButton);
                            loadCheck(pc, listaCmp.Count, listaCmpCheck);
                            break;
                        case "TRD":
                            listaTrd.Add(pc);
                            loadBtn(pc, listaTrd.Count, listaTrdButton);
                            loadCheck(pc, listaTrd.Count, listaTrdCheck);
                            break;
                        case "DOK":
                            listaDok.Add(pc);
                            loadBtn(pc, listaDok.Count, listaDokButton);
                            loadCheck(pc, listaDok.Count, listaDokCheck);
                            break;
                        case "SERVER":
                            listaSrv.Add(pc);
                            loadBtn(pc, listaSrv.Count, listaSrvButton);
                            loadCheck(pc, listaSrv.Count, listaSrvCheck);
                            break;
                        case "GW":
                            listaGw.Add(pc);
                            loadBtn(pc, listaGw.Count, listaGwButton);
                            loadCheck(pc, listaGw.Count, listaGwCheck);
                            break;
                        case "MFC":
                            listaMfc.Add(pc);
                            loadBtn(pc, listaMfc.Count, listaMfcButton);
                            loadCheck(pc, listaMfc.Count, listaMfcCheck);
                            break;

                        default:
                            MessageBox.Show("Error Type in file xml\n Consulta i tipi consentiti");
                            return;
                    }

                }
            }
        }


        public Button loadBtn( PC pc, int count, ArrayList list)
        {

                list.Add(new Button());
            string TYPE = pc.Type.ToUpper();

            switch (TYPE)
            {
                case "GW":
                    {
                        int nFile = (list.Count / 2) + 1;
                        int nRiga = (list.Count - 1) / 2;

                        int saltoriga = 5 + (105 * nRiga);

                        int countNuovaRiga = list.Count - 1 - (nRiga * 2);


                        (list[list.Count - 1] as Button).Location = new Point(saltoriga, 5 + (countNuovaRiga) * 30);
                        (list[list.Count - 1] as Button).BackColor = Color.LightGray;
                        (list[list.Count - 1] as Button).Name = pc.Nome;
                        (list[list.Count - 1] as Button).Text = pc.Nome.ToUpper();
                        (list[list.Count - 1] as Button).Size = new Size(80, 30);
                        (list[list.Count - 1] as Button).Tag = pc.Type;
                        (list[list.Count - 1] as Button).Click += new EventHandler(ClickRunTime);
                        break;
                    }
                case "MFC":
                    {
                        int nFile1 = (list.Count / 8) + 1;
                        int nRiga1 = (list.Count - 1) / 8;

                        int saltoriga1 = 5 + (105 * nRiga1);

                        int countNuovaRiga1 = list.Count - 1 - (nRiga1 * 8);


                        (list[list.Count - 1] as Button).Location = new Point(saltoriga1, 5 + (countNuovaRiga1) * 30);
                        (list[list.Count - 1] as Button).BackColor = Color.LightGray;
                        (list[list.Count - 1] as Button).Name = pc.Nome;
                        (list[list.Count - 1] as Button).Text = pc.Nome.ToUpper();
                        (list[list.Count - 1] as Button).Size = new Size(80, 30);
                        (list[list.Count - 1] as Button).Tag = pc.Type;
                        (list[list.Count - 1] as Button).Click += new EventHandler(ClickRunTime);
                        break;
                    }

                default:
                    {
                        int nFile2 = (list.Count / 6) + 1;
                        int nRiga2 = (list.Count - 1) / 6;

                        int saltoriga2 = 5 + (105 * nRiga2);

                        int countNuovaRiga2 = list.Count - 1 - (nRiga2 * 6);


                        (list[list.Count - 1] as Button).Location = new Point(saltoriga2, 5 + (countNuovaRiga2) * 30);
                        (list[list.Count - 1] as Button).BackColor = Color.LightGray;
                        (list[list.Count - 1] as Button).Name = pc.Nome;
                        (list[list.Count - 1] as Button).Text = pc.Nome.ToUpper();
                        (list[list.Count - 1] as Button).Size = new Size(80, 30);
                        (list[list.Count - 1] as Button).Tag = pc.Type;
                        (list[list.Count - 1] as Button).Click += new EventHandler(ClickRunTime);
                    };
                    break;

            }
            return (list[list.Count - 1] as Button);
        }

        public void ClickRunTime(object sender, EventArgs e)
        {
    
                if (Form1.getQualeradio == 0) { 

                new Funzioni().openFolder(listamacchine, sender);


                }
                else {
                new Funzioni().openRmDesk(listamacchine, sender);
                }
        }


        public CheckBox loadCheck(PC pc, int count, ArrayList list)
        {
            string TYPE = pc.Type.ToUpper();

            switch (TYPE)
            {
                case "GW":
                    {
                        list.Add(new CheckBox());

                        int nFile = (list.Count / 2) + 1;
                        int nRiga = (list.Count - 1) / 2;

                        int saltoriga = 90 + (105 * nRiga);

                        int countNuovaRiga = list.Count - 1 - (nRiga * 2);

                        (list[list.Count - 1] as CheckBox).Location = new Point(saltoriga, 15 + (countNuovaRiga) * 30);
                        (list[list.Count - 1] as CheckBox).Name = pc.Nome;
                        (list[list.Count - 1] as CheckBox).Text = "";
                        (list[list.Count - 1] as CheckBox).Size = new Size(15, 15);
                        (list[list.Count - 1] as CheckBox).Tag = pc.Type;
                    }
                    break;

                case "MFC":
                    {
                        list.Add(new CheckBox());

                        int nFile = (list.Count / 8) + 1;
                        int nRiga = (list.Count - 1) / 8;

                        int saltoriga = 90 + (105 * nRiga);

                        int countNuovaRiga = list.Count - 1 - (nRiga * 8);

                        (list[list.Count - 1] as CheckBox).Location = new Point(saltoriga, 15 + (countNuovaRiga) * 30);
                        (list[list.Count - 1] as CheckBox).Name = pc.Nome;
                        (list[list.Count - 1] as CheckBox).Text = "";
                        (list[list.Count - 1] as CheckBox).Size = new Size(15, 15);
                        (list[list.Count - 1] as CheckBox).Tag = pc.Type;
                    } break;

                default:
                    {
                        list.Add(new CheckBox());

                        int nFile = (list.Count / 6) + 1;
                        int nRiga = (list.Count - 1) / 6;

                        int saltoriga = 90 + (105 * nRiga);

                        int countNuovaRiga = list.Count - 1 - (nRiga * 6);

                        (list[list.Count - 1] as CheckBox).Location = new Point(saltoriga, 15 + (countNuovaRiga) * 30);
                        (list[list.Count - 1] as CheckBox).Name = pc.Nome;
                        (list[list.Count - 1] as CheckBox).Text = "";
                        (list[list.Count - 1] as CheckBox).Size = new Size(15, 15);
                        (list[list.Count - 1] as CheckBox).Tag = pc.Type;
                    } break ;
            }
            return list[list.Count - 1] as CheckBox;
        }

        public void loadListPC()
        {

            foreach (PC pc in listaCmp)
            {
                listamacchine.Add(pc);
            }
            foreach (PC pc in listaTrd)
            {
                listamacchine.Add(pc);
            }
            foreach (PC pc in listaDok)
            {
                listamacchine.Add(pc);
            }
            foreach (PC pc in listaSrv)
            {
                listamacchine.Add(pc);
            }
            foreach (PC pc in listaGw)
            {
                listamacchine.Add(pc);
            }
            foreach (PC pc in listaMfc)
            {
                listamacchine.Add(pc);
            }

        }
        public void loadListBTN()
        {
            foreach (Button btn in listaCmpButton)
            {
                listaButton.Add(btn);
            }
            foreach (Button btn in listaTrdButton)
            {
                listaButton.Add(btn);
            }
            foreach (Button btn in listaDokButton)
            {
                listaButton.Add(btn);
            }
            foreach (Button btn in listaSrvButton)
            {
                listaButton.Add(btn);
            }
            foreach (Button btn in listaGwButton)
            {
                listaButton.Add(btn);
            }
            foreach (Button btn in listaMfcButton)
            {
                listaButton.Add(btn);
            }
        }
        public void loadListCHK() {

            foreach (CheckBox btn in listaCmpCheck)
            {
                listaCheck.Add(btn);
            }
            foreach (CheckBox btn in listaTrdCheck)
            {
                listaCheck.Add(btn);
            }
            foreach (CheckBox btn in listaDokCheck)
            {
                listaCheck.Add(btn);
            }
            foreach (CheckBox btn in listaSrvCheck)
            {
                listaCheck.Add(btn);
            }
            foreach (CheckBox btn in listaGwCheck)
            {
                listaCheck.Add(btn);
            }
            foreach (CheckBox btn in listaMfcCheck)
            {
                listaCheck.Add(btn);
            }
        }

        
            

    } 

}

