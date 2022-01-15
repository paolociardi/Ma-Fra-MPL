using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.AccessControl;
using System.Globalization;

namespace Ma_Fra_MPL
{
    public partial class Form1 : Form
    {
        string magazzino_csv = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/MaFraMPL/magazzino.csv";
        string clienti_csv = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/MaFraMPL/clienti.csv";
        string archivio_csv = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/MaFraMPL/archivio.csv";
        string backup_txt = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/MaFraMPL/backup.txt";

        string totale_imponibile = "0.00", calcolo_imposta = "0.00", percentuale_iva = "0.00";
        string ddt_nome_cliente = string.Empty, ddt_lemma_cliente = string.Empty; string ddt_indirizzo_cliente = string.Empty, ddt_citta_cliente = string.Empty, ddt_destinazione_cliente = string.Empty, ddt_codice_cliente = string.Empty, ddt_partita_iva_cliente = string.Empty, ddt_codice_fiscale_cliente = string.Empty;
        
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)                                         //Codice da eseuire al caricamento del form
        {
            //Imposta il punto come separatore decimale accettato al posto della virgola
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            Console.WriteLine(DateTime.Now.TimeOfDay.Hours.ToString("00") + DateTime.Now.TimeOfDay.Minutes.ToString("00") + DateTime.Now.TimeOfDay.Seconds.ToString("00"));

            checkAppdata();
            clientiFillTable(clienti_csv);
            magazzinoFillTable(magazzino_csv);
            archivioFillTable(archivio_csv);
            txtData.Value = DateTime.Today;
            txtOra.Value = DateTime.Now.TimeOfDay.Hours;
            lblTime.Text = ":" + DateTime.Now.TimeOfDay.Minutes.ToString("00") + ":" + DateTime.Now.TimeOfDay.Seconds.ToString("00");
            bollaBindingSource.DataSource = new List<Bolla>();
        }
        private void checkAppdata()                                                                 //Verifica la presenza dei file necessari al funzionamento del software in cartella %APPDATA%
        {
            if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/MaFraMPL"))
                System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/MaFraMPL");
            filesCheck(clienti_csv);
            filesCheck(magazzino_csv);
            filesCheck(archivio_csv);
        }
        private void btnAggiungiBolla_Click(object sender, EventArgs e)                             //Aggiunge prodotto al D.D.T. che si sta generando
        {
            if (!string.IsNullOrWhiteSpace(Convert.ToString(txtCodiceProdotto.Text)) && !string.IsNullOrWhiteSpace(Convert.ToString(txtNomeProdotto.Text)) && !string.IsNullOrWhiteSpace(Convert.ToString(txtUnitaMisura.Text)) && txtPrezzo.Value >= 0 && txtQuantita.Value > 0)
            {
                Bolla obj = new Bolla() { codice = txtCodiceProdotto.Text.ToUpper(), descrizione = txtNomeProdotto.Text, unita_misura = txtUnitaMisura.Text, prezzo = (Math.Round(txtPrezzo.Value, 2, MidpointRounding.AwayFromZero)).ToString("0.00"), quantita = Convert.ToInt32(txtQuantita.Value).ToString(), importo = Math.Round((Convert.ToDouble(txtPrezzo.Text) * Convert.ToInt32(txtQuantita.Text)), 2, MidpointRounding.AwayFromZero).ToString("0.00"), c = Convert.ToString(Convert.ToDouble(percentuale_iva)) };
                totale_imponibile = (Math.Round((Convert.ToDouble(totale_imponibile) + Convert.ToDouble(obj.importo)), 2, MidpointRounding.AwayFromZero)).ToString("0.00");
                if (!bollaCheckDouble(obj))
                {
                    bollaBindingSource.Add(obj);
                    mgzQuantitaProdotti(Convert.ToString(obj.codice.ToUpper()), true, Convert.ToInt32(obj.quantita));
                }
                else { MessageBox.Show("Il D.D.T. contiene già un prodotto con il codice \"" + Convert.ToString(obj.codice) + "\".", "ATTENZIONE"); }
                txtBollaClean();
            }
            else { MessageBox.Show("Compilare correttamente tutti i campi.", "ATTENZIONE"); }
        }
        private bool bollaCheckDouble(Bolla obj)                                                    //Verifica se il D.D.T. contiene già il prodotto che si sta cercando di aggiungere
        {
            for (int i = 0; i < dataGridBolla.RowCount; i++)
            {
                if (obj.codice.ToString().Equals(dataGridBolla[0, i].Value.ToString()))
                    return true;
            }
            return false;
        }
        private void btnRimuoviBolla_Click(object sender, EventArgs e)                              //Rimuove prodotto dal D.D.T. che si sta generando
        {
            Bolla obj = bollaBindingSource.Current as Bolla;
            if (obj != null)
            {
                totale_imponibile = (Convert.ToDouble(totale_imponibile) - Convert.ToDouble(obj.importo)).ToString("0.00");
                mgzQuantitaProdotti(Convert.ToString(obj.codice.ToUpper()), false, Convert.ToInt32(obj.quantita));
            }
            bollaBindingSource.RemoveCurrent();
        }
        private void btnStampaBolla_Click(object sender, EventArgs e)                               //Genera e stampa il Documento di Trasporto
        {
            if (!string.IsNullOrWhiteSpace(txtNomeCliente.Text) && txtNumeroDocumento.Value != 0 && bollaBindingSource.Count > 0 && !string.IsNullOrWhiteSpace(txtAspettoEsterioreBeni.Text))
            {
                percentuale_iva = txtPercentualeIva.Value.ToString("0.00");
                calcolo_imposta = (Math.Round((Convert.ToDouble(percentuale_iva) * Convert.ToDouble(totale_imponibile) / 100), 2, MidpointRounding.AwayFromZero)).ToString("0.00");
                string ora = txtOra.Value.ToString("00") + Convert.ToString(lblTime.Text);
                ddt_destinazione_cliente = string.IsNullOrWhiteSpace(ddt_destinazione_cliente) ? " " : "Destinazione: " + ddt_destinazione_cliente;
                ddt_codice_fiscale_cliente = string.IsNullOrWhiteSpace(ddt_codice_fiscale_cliente) ? " " : ddt_codice_fiscale_cliente;
                using (LayoutBolla frm = new LayoutBolla(bollaBindingSource.DataSource as List<Bolla>, "Spett." + ddt_nome_cliente, ddt_lemma_cliente, ddt_indirizzo_cliente, ddt_citta_cliente, ddt_destinazione_cliente, Convert.ToString(txtNumeroDocumento.Value), Convert.ToString(txtData.Text), Convert.ToString(txtAspettoEsterioreBeni.Text), "Vendita", Convert.ToString(txtNumeroColli.Text), ddt_codice_cliente, ddt_partita_iva_cliente, ddt_codice_fiscale_cliente, Convert.ToString(txtData.Text), ora, txtPercentualeIva.Value.ToString("0.00"), Math.Round(Convert.ToDouble(totale_imponibile), 2, MidpointRounding.AwayFromZero).ToString("0.00"), (Math.Round(Convert.ToDouble(calcolo_imposta), 2, MidpointRounding.AwayFromZero)).ToString("0.00"), (Math.Round(Convert.ToDouble(totale_imponibile) + Convert.ToDouble(calcolo_imposta), 2, MidpointRounding.AwayFromZero)).ToString("0.00"), checkBoxSenzaPrezzo.Checked, archivio_csv))
                {
                    frm.ShowDialog();
                }
                totale_imponibile = "0.00";
                bollaBindingSource.Clear();
                lblTime.Text = ":" + DateTime.Now.TimeOfDay.Minutes.ToString("00") + ":" + DateTime.Now.TimeOfDay.Seconds.ToString("00");
                txtNomeCliente.Text = string.Empty;
                checkBoxSenzaPrezzo.Checked = false;
                archivioFillTable(archivio_csv);
            }
            else { MessageBox.Show("Assicurarsi di aver compilato correttamente tutti i campi e di avere inserito almeno un prodotto al Documento di Trasporto.", "ATTENZIONE"); }
        }
        private void filesCheck(string file_path)                                                   //Verifica la presenza dei .csv di sistema, in caso assenti li genera
        {
            if (!System.IO.File.Exists(file_path))
                System.IO.File.Create(file_path).Close();
        }
        private void btnMgzAggiungi_Click(object sender, EventArgs e)                               //Aggiungi il prodotto specificato al magazzino
        {
            if ((!string.IsNullOrWhiteSpace(txtMgzCodice.Text)) && (!string.IsNullOrWhiteSpace(txtMgzProdotto.Text)) && (!string.IsNullOrWhiteSpace(txtMgzUnitaMisura.Text)) && (!string.IsNullOrWhiteSpace(txtMgzAvviso.Text)) && (txtMgzUnitaMisura.Text.Equals("PZ") || txtMgzUnitaMisura.Text.Equals("KG") || txtMgzUnitaMisura.Text.Equals("LT")))
            {
                if (!checkForDouble(txtMgzCodice.Text, magazzino_csv))
                {
                    string nuovo_prodotto = $"{txtMgzCodice.Text.ToUpper()};{txtMgzProdotto.Text};{Convert.ToInt32(txtMgzQuantita.Value).ToString()};{txtMgzUnitaMisura.Text.ToUpper()};{txtMgzPrezzo.Value.ToString("0.00")};{txtMgzAvviso.Value.ToString("0.00")}\n";
                    System.IO.File.AppendAllText(magazzino_csv, nuovo_prodotto);
                    string[] data = nuovo_prodotto.Split(';');
                    tblMagazzino.Rows.Add(data);
                    tblMgzControlloAvviso(); 
                }
                else
                    MessageBox.Show("Il magazzino contiene già un prodotto con lo stesso codice", "ATTENZIONE");
            }
            else
                MessageBox.Show("E' necessario compilare correttamente tutti i campi per aggiungere un prodotto al magazzino.", "ATTENZIONE");
            txtMgzClean();
        }
        private void magazzinoFillTable(string fileName)                                            //Popola la tabella nella tab "Magazzino" da file magazzino.csv
        {
            string[] lines = System.IO.File.ReadAllLines(fileName);
            tblMagazzino.Rows.Clear();
            foreach (string line in lines)
            {
                string[] data = line.Split(';');
                tblMagazzino.Rows.Add(data);
            }
            tblMgzControlloAvviso();
        }
        private void clientiFillTable(string fileName)                                              //Popola la tabella nella tab "Clienti" da file clienti.csv
        {
            string[] lines = System.IO.File.ReadAllLines(fileName);
            tblClienti.Rows.Clear();
            foreach (string line in lines)
            {
                string[] data = line.Split(';');
                tblClienti.Rows.Add(data);
            }
        }
        private void archivioFillTable(string fileName)                                             //Popola la tabella in Archivio e prepara i campi filtro
        {
            txtArcNumeroDocumento.Value = 0;
            txtArcCliente.Text = string.Empty;
            DateTime data_minima = DateTime.Today;
            DateTime data_massima = DateTime.Today;
            string[] lines = System.IO.File.ReadAllLines(fileName);
            tblArchivio.Rows.Clear();
            decimal temp_percentuale_iva = 0;
            foreach (string line in lines)
            {
                string[] data = line.Split(';');
                if (data.Length > 10)
                {
                    string[] table = { data[0], data[1], data[2], data[5] };
                    tblArchivio.Rows.Add(table);
                    data_minima = Convert.ToDateTime(data[2]) < data_minima ? Convert.ToDateTime(data[2]) : data_minima;
                    data_massima = Convert.ToDateTime(data[2]) > data_massima ? Convert.ToDateTime(data[2]) : data_massima;
                    temp_percentuale_iva = Convert.ToDecimal(data[6]); 
                }
            }
            txtArcDataDal.MaxDate = data_massima;
            txtArcDataDal.MinDate = data_minima;
            txtArcDataDal.Value = txtArcDataDal.MinDate;
            txtArcDataAl.MaxDate = data_massima;
            txtArcDataAl.MinDate = data_minima;
            txtArcDataAl.Value = txtArcDataAl.MaxDate;
            panel1.Visible = tblArchivio.RowCount > 0 ? true : false;
            txtNumeroDocumento.Value = tblArchivio.RowCount > 0 ? Convert.ToDecimal(tblArchivio.Rows[tblArchivio.RowCount - 1].Cells[0].Value) + 1 : 1;
            txtPercentualeIva.Value = temp_percentuale_iva;
        }
        private void txtClnClean()                                                                  //Inizializza i campi di testo della tab "Magazzino"
        {
            txtClnCodice.Text = string.Empty;
            txtClnLemma.Text = string.Empty;
            txtClnNome.Text = string.Empty;
            txtClnPartitaIva.Text = string.Empty;
            txtClnCodiceFiscale.Text = string.Empty;
            txtClnIndirizzo.Text = string.Empty;
            txtClnIndirizzo2.Text = string.Empty;
            txtClnIndirizzo3.Text = string.Empty;
        }
        private void txtMgzClean()                                                                  //Inizializza i campi di testo della tab "Magazzino"
        {
            txtMgzCodice.Text = string.Empty;
            txtMgzProdotto.Text = string.Empty;
            txtMgzQuantita.Value = 0;
            txtMgzUnitaMisura.Text = string.Empty;
            txtMgzPrezzo.Value = 0;
            txtMgzAvviso.Value = 0;
        }
        private void txtBollaClean()                                                                //Inizializza i campi di testo della sezione prodotti in tab "Genera DDT"
        {
            txtCodiceProdotto.Text = string.Empty;
            txtNomeProdotto.Text = string.Empty;
            txtQuantita.Value = 0;
            txtPrezzo.Value = 0;
            txtUnitaMisura.SelectedIndex = 0;
        }
        private void tblMgzControlloAvviso()                                                        //Evidenzia le righe della tabella prodotti in relazione alla loro quantità
        {
            foreach (DataGridViewRow row in tblMagazzino.Rows)
            {
                if (Convert.ToDouble(row.Cells[2].Value) <= Convert.ToDouble(row.Cells[5].Value))
                    row.DefaultCellStyle.BackColor = Color.Silver;
                if (Convert.ToDouble(row.Cells[2].Value) <= 0)
                    row.DefaultCellStyle.BackColor = Color.Salmon;
            }
        }
        private void btnMgzRimuovi_Click(object sender, EventArgs e)                                //Rimuovi prodotto dalla tabella Magazzino
        {
            if (!string.IsNullOrWhiteSpace(txtMgzCodice.Text))
            {
                string[] lines = System.IO.File.ReadAllLines(magazzino_csv);
                System.IO.File.WriteAllText(magazzino_csv, string.Empty);
                foreach (string line in lines)
                {
                    string[] data = line.Split(';');
                    if (data[0].Equals(txtMgzCodice.Text.ToUpper()))
                    {
                        DialogResult dialogResult = MessageBox.Show("Rimuovere definitivamente il prodotto: " + data[1] + "?", "RIMUOVI", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.No)
                        {
                            System.IO.File.AppendAllText(magazzino_csv, line + "\n");
                        }
                    }
                    else { System.IO.File.AppendAllText(magazzino_csv, line + "\n"); }
                }
                magazzinoFillTable(magazzino_csv);
            }
            else
                MessageBox.Show("E' necessario compilare correttamente il campo \"Codice\" per rimuovere il relativo prodotto dal magazzino.", "ATTENZIONE");
            txtMgzClean();
        }
        private void btnMgzModifica_Click(object sender, EventArgs e)                               //Modifica prodotto in tabella Magazzino
        {
            if ((!string.IsNullOrWhiteSpace(txtMgzCodice.Text)) && (!string.IsNullOrWhiteSpace(txtMgzProdotto.Text)) && (!string.IsNullOrWhiteSpace(txtMgzUnitaMisura.Text)) && (!string.IsNullOrWhiteSpace(txtMgzAvviso.Text)) && (txtMgzUnitaMisura.Text.Equals("PZ") || txtMgzUnitaMisura.Text.Equals("KG") || txtMgzUnitaMisura.Text.Equals("LT")))
            {
                string[] lines = System.IO.File.ReadAllLines(magazzino_csv);
                System.IO.File.WriteAllText(magazzino_csv, string.Empty);
                foreach (string line in lines)
                {
                    string[] data = line.Split(';');
                    if (data[0].Equals(txtMgzCodice.Text.ToUpper()))
                    {
                        DialogResult dialogResult = MessageBox.Show($"Il prodotto sta per subire le seguenti modifiche:\n\nCodice: {data[0]} -> {txtMgzCodice.Text.ToUpper()}\nNome: {data[1]} -> {txtMgzProdotto.Text}\nQuantità: {data[2]} -> {Convert.ToString(txtMgzQuantita.Value)}\nUnità di Misura: {data[3]} -> {txtMgzUnitaMisura.Text}\nPrezzo: {data[4]} -> {Convert.ToString(txtMgzPrezzo.Value)}\nAvviso: {data[5]} -> {Convert.ToString(txtMgzAvviso.Value)}\n\nContinuare?", "MODIFICA", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            string prodotto_modificato = $"{txtMgzCodice.Text.ToUpper()};{txtMgzProdotto.Text};{Convert.ToInt32(txtMgzQuantita.Value).ToString()};{txtMgzUnitaMisura.Text.ToUpper()};{txtMgzPrezzo.Value.ToString("0.00")};{txtMgzAvviso.Value.ToString("0.00")}";
                            System.IO.File.AppendAllText(magazzino_csv, prodotto_modificato + "\n");
                        }
                        else { System.IO.File.AppendAllText(magazzino_csv, line + "\n"); }
                    }
                    else { System.IO.File.AppendAllText(magazzino_csv, line + "\n"); }
                }
                magazzinoFillTable(magazzino_csv);
            }
            else
                MessageBox.Show("E' necessario compilare correttamente tutti i campi per modificare un prodotto.", "ATTENZIONE");
            txtMgzClean();
        }               
        private void btnClnRimuovi_Click(object sender, EventArgs e)                                //Alla pressione del tasto Rimuovi cancella il cliente specificato dalla tabella Clienti
        {
            if (!string.IsNullOrWhiteSpace(txtClnCodice.Text))
            {
                string[] lines = System.IO.File.ReadAllLines(clienti_csv);
                System.IO.File.WriteAllText(clienti_csv, string.Empty);
                foreach (string line in lines)
                {
                    string[] data = line.Split(';');
                    if (data[0].Equals(txtClnCodice.Text.ToUpper()))
                    {
                        DialogResult dialogResult = MessageBox.Show("Rimuovere definitivamente il cliente: " + data[2] + "?", "RIMUOVI", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.No)
                            System.IO.File.AppendAllText(clienti_csv, line + "\n");
                    }
                    else { System.IO.File.AppendAllText(clienti_csv, line + "\n"); }
                }
                clientiFillTable(clienti_csv);
            }
            else { MessageBox.Show("E' necessario compilare correttamente il campo \"Codice\" per rimuovere il relativo prodotto dal magazzino.", "ATTENZIONE"); }
            txtClnClean();
        }
        private void btnClnModifica_Click(object sender, EventArgs e)                               //Modifica cliente in tabella Clienti
        {
            if ((!string.IsNullOrWhiteSpace(txtClnCodice.Text)) && (!string.IsNullOrWhiteSpace(txtClnNome.Text)) && (!string.IsNullOrWhiteSpace(txtClnPartitaIva.Text)) && (!string.IsNullOrWhiteSpace(txtClnIndirizzo.Text)) && (!string.IsNullOrWhiteSpace(txtClnIndirizzo2.Text)))
            {
                string[] lines = System.IO.File.ReadAllLines(clienti_csv);
                System.IO.File.WriteAllText(clienti_csv, string.Empty);
                foreach (string line in lines)
                {
                    string[] data = line.Split(';');
                    if (data[0].Equals(txtClnCodice.Text.ToUpper()))
                    {
                        DialogResult dialogResult = MessageBox.Show($"Il cliente con codice \"{txtClnCodice.Text.ToUpper()}\"sta per subire le seguenti modifiche:\n\nLemma: {data[1]} -> {txtClnLemma.Text}\nNome: {data[2]} -> {txtClnNome.Text}\nPartita Iva: {data[3]} -> {txtClnPartitaIva.Text}\nCodice Fiscale: {data[4]} -> {txtClnCodiceFiscale.Text}\nIndirizzo: {data[5]} -> {txtClnIndirizzo.Text}\nCittà: {data[6]} -> {txtClnIndirizzo2.Text}\nDestinazione: {data[7]} -> {txtClnIndirizzo3.Text}\n\nContinuare?", "MODIFICA", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            string lemma = string.IsNullOrWhiteSpace(txtClnLemma.Text) ? txtClnNome.Text : txtClnLemma.Text;
                            string cliente_modificato = $"{txtClnCodice.Text.ToUpper()};{lemma};{txtClnNome.Text};{txtClnPartitaIva.Text};{txtClnCodiceFiscale.Text};{txtClnIndirizzo.Text};{txtClnIndirizzo2.Text};{txtClnIndirizzo3.Text}";
                            System.IO.File.AppendAllText(clienti_csv, cliente_modificato + "\n");
                        }
                        else { System.IO.File.AppendAllText(clienti_csv, line + "\n"); }
                    }
                    else { System.IO.File.AppendAllText(clienti_csv, line + "\n"); }
                }
                clientiFillTable(clienti_csv);
            }
            else { MessageBox.Show("E' necessario compilare correttamente tutti i campi per modificare un cliente.", "ATTENZIONE"); }
            txtClnClean();
        }
        private void btnClnAggiungi_Click(object sender, EventArgs e)                               //Aggiungi il cliente specificato alla rubrica
        {
            if ((!string.IsNullOrWhiteSpace(txtClnCodice.Text)) && (!string.IsNullOrWhiteSpace(txtClnNome.Text)) && (!string.IsNullOrWhiteSpace(txtClnPartitaIva.Text)) && (!string.IsNullOrWhiteSpace(txtClnIndirizzo.Text)) && (!string.IsNullOrWhiteSpace(txtClnIndirizzo2.Text)))
            {
                if (!checkForDouble(txtClnCodice.Text, clienti_csv))
                {
                    string lemma = string.IsNullOrWhiteSpace(txtClnLemma.Text) ? txtClnNome.Text : txtClnLemma.Text;
                    string nuovo_cliente = $"{txtClnCodice.Text.ToUpper()};{lemma};{txtClnNome.Text};{txtClnPartitaIva.Text};{txtClnCodiceFiscale.Text.ToUpper()};{txtClnIndirizzo.Text};{txtClnIndirizzo2.Text};{txtClnIndirizzo3.Text}\n";
                    System.IO.File.AppendAllText(clienti_csv, nuovo_cliente);
                    clientiFillTable(clienti_csv);
                }
                else { MessageBox.Show("La rubrica contiene già un cliente con lo stesso codice.", "ATTENZIONE"); }
            }
            else { MessageBox.Show("E' necessario compilare correttamente tutti i campi per aggiungere un cliente alla rubrica.", "ATTENZIONE"); }
            txtClnClean();
        }
        private void txtNomeCliente_Click(object sender, EventArgs e)                               //Al click della textbox Cliente in tab "Genera DDT" rimanda alla tab "Clienti" per consentire la selezione
        {
            tabControl1.SelectedTab = clienti;
        }
        private bool checkForDouble(string codice, string file_path)                                //Restituisce "true" in caso si verifichino doppioni all'inserimento di un nuovo cliente o prodotto
        {
            string[] lines = System.IO.File.ReadAllLines(file_path);
            foreach (string line in lines)
            {
                string[] data = line.Split(';');
                if (data[0].Equals(codice.ToUpper()))
                    return true;
            }
            return false;
        }
        private void tblClienti_CellClick(object sender, DataGridViewCellEventArgs e)               //Autofill dei campi cliente in base all'elemento selezionato in tabella
        {
            if(e.RowIndex >= 0)
            {
                txtClnCodice.Text = tblClienti.Rows[e.RowIndex].Cells[0].Value.ToString();
                txtClnLemma.Text = tblClienti.Rows[e.RowIndex].Cells[1].Value.ToString();
                txtClnNome.Text = tblClienti.Rows[e.RowIndex].Cells[2].Value.ToString();
                txtClnPartitaIva.Text = tblClienti.Rows[e.RowIndex].Cells[3].Value.ToString();
                txtClnCodiceFiscale.Text = tblClienti.Rows[e.RowIndex].Cells[4].Value.ToString();
                txtClnIndirizzo.Text = tblClienti.Rows[e.RowIndex].Cells[5].Value.ToString();
                txtClnIndirizzo2.Text = tblClienti.Rows[e.RowIndex].Cells[6].Value.ToString();
                txtClnIndirizzo3.Text = tblClienti.Rows[e.RowIndex].Cells[7].Value.ToString();
            }
        }
        private void tblMagazzino_CellClick(object sender, DataGridViewCellEventArgs e)             //Autofille dei campi prodotto in base all'elemento selezionato in tabella
        {
            if (e.RowIndex >= 0)
            {
                txtMgzCodice.Text = tblMagazzino.Rows[e.RowIndex].Cells[0].Value.ToString();
                txtMgzProdotto.Text = tblMagazzino.Rows[e.RowIndex].Cells[1].Value.ToString();
                txtMgzQuantita.Value = Convert.ToDecimal(tblMagazzino.Rows[e.RowIndex].Cells[2].Value.ToString());
                txtMgzUnitaMisura.Text = tblMagazzino.Rows[e.RowIndex].Cells[3].Value.ToString();
                txtMgzPrezzo.Value = Convert.ToDecimal(tblMagazzino.Rows[e.RowIndex].Cells[4].Value.ToString());
                txtMgzAvviso.Value = Convert.ToDecimal(tblMagazzino.Rows[e.RowIndex].Cells[5].Value.ToString());
            }
        }
        private void txtClnCodice_TextChanged(object sender, EventArgs e)                           //Filtra gli elementi in tabella in base al codice cliente
        {
            if (txtClnCodice.Focused)
            {
                if (!string.IsNullOrWhiteSpace(txtClnCodice.Text))
                {
                    string searchValue = txtClnCodice.Text.ToUpper();
                    string[] lines = System.IO.File.ReadAllLines(clienti_csv);
                    tblClienti.Rows.Clear();
                    foreach (string line in lines)
                    {
                        string[] data = line.Split(';');
                        if (data[0].ToUpper().Contains(searchValue))
                            tblClienti.Rows.Add(data);
                    }
                }
                else { clientiFillTable(clienti_csv); } 
            }
        }
        private void txtClnLemma_TextChanged(object sender, EventArgs e)                            //Filtra gli elementi in tabella in base al lemma
        {
            if (txtClnLemma.Focused)
            {
                if (!string.IsNullOrWhiteSpace(txtClnLemma.Text))
                {
                    string searchValue = txtClnLemma.Text.ToUpper();
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(clienti_csv);
                        tblClienti.Rows.Clear();
                        foreach (string line in lines)
                        {
                            string[] data = line.Split(';');
                            if (data[1].ToUpper().Contains(searchValue))
                                tblClienti.Rows.Add(data);
                        }
                    }
                    catch (Exception exc) { MessageBox.Show(exc.Message); }
                }
                else { clientiFillTable(clienti_csv); }
            }
        }
        private void txtMgzCodice_TextChanged(object sender, EventArgs e)                           //Filtra gli elementi in tabella in base al codice prodotto
        {
            if (txtMgzCodice.Focused)
            {
                if (!string.IsNullOrWhiteSpace(txtMgzCodice.Text))
                {
                    string searchValue = txtMgzCodice.Text.ToUpper();
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(magazzino_csv);
                        tblMagazzino.Rows.Clear();
                        foreach (string line in lines)
                        {
                            string[] data = line.Split(';');
                            if (data[0].ToUpper().Contains(searchValue))
                                tblMagazzino.Rows.Add(data);
                        }
                    }
                    catch (Exception exc) { MessageBox.Show(exc.Message); }
                }
                else { magazzinoFillTable(magazzino_csv); }
            }
        }
        private void txtArcNumeroDocumento_ValueChanged(object sender, EventArgs e)                 //Filtra gli elementi in tabella in base a numero, cliente e data
        {
            if (txtArcNumeroDocumento.Focused || txtArcCliente.Focused || txtArcDataDal.Focused || txtArcDataAl.Focused)
            {
                if (txtArcNumeroDocumento.Value > 0 || !string.IsNullOrWhiteSpace(txtArcCliente.Text) || txtArcDataDal.Value > txtArcDataDal.MinDate || txtArcDataAl.Value < txtArcDataAl.MaxDate)
                {
                    string[] lines = System.IO.File.ReadAllLines(archivio_csv);
                    tblArchivio.Rows.Clear();
                    if (lines.Length > 0)
                    {
                        foreach (string line in lines)
                        {
                            string[] data = line.Split(';');
                            if (data.Length > 10)
                            {
                                if ((txtArcNumeroDocumento.Value == 0 || data[0].StartsWith(txtArcNumeroDocumento.Value.ToString())) && (string.IsNullOrEmpty(txtArcCliente.Text) || data[1].ToUpper().Contains(txtArcCliente.Text.ToUpper())) && Convert.ToDateTime(data[2]) >= txtArcDataDal.Value && Convert.ToDateTime(data[2]) <= txtArcDataAl.Value)
                                {
                                    string[] table = { data[0], data[1], data[2], data[5] };
                                    tblArchivio.Rows.Add(table);
                                } 
                            }
                        } 
                    }
                }
                else if (txtArcNumeroDocumento.Value == 0 && string.IsNullOrWhiteSpace(txtArcCliente.Text) && txtArcDataDal.Value == txtArcDataDal.MinDate || txtArcDataAl.Value == txtArcDataAl.MaxDate)
                    archivioFillTable(archivio_csv);
            }
            panel1.Visible = tblArchivio.RowCount == 0 ? false : true;
        }
        private void btnArcReload_Click(object sender, EventArgs e)                                 //Azzera i filtri nel tab Archivio mostrando tutto lo storico bolle
        {
            archivioFillTable(archivio_csv);
        }
        private void tblArchivio_SelectionChanged(object sender, EventArgs e)                       //Mostra nella parte inferiore del tab Archivio il DDT selezionato nella parte superiore
        {
            string[] lines = System.IO.File.ReadAllLines(archivio_csv);
            foreach (string line in lines)
            {
                string[] data = line.Split(';');
                if (data[0].Equals(tblArchivio.Rows[tblArchivio.CurrentRow.Index].Cells[0].Value.ToString()) && data[1].Equals(tblArchivio.Rows[tblArchivio.CurrentRow.Index].Cells[1].Value.ToString()) && data[2].Equals(tblArchivio.Rows[tblArchivio.CurrentRow.Index].Cells[2].Value.ToString()) && data[5].Equals(tblArchivio.Rows[tblArchivio.CurrentRow.Index].Cells[3].Value.ToString()))
                {
                    lblArcNumero.Text = data[0];
                    lblArcCliente.Text = data[1];
                    lblArcData.Text = data[2];
                    lblArcImponibile.Text = data[3];
                    lblArcImposta.Text = data[4];
                    lblArcTotale.Text = data[5];
                    lblArcProdotti.Text = data[7];
                    tblArcProdotti.Rows.Clear();
                    for (UInt16 i = 7; i < (7 + (5 * Convert.ToUInt16(data[7]))); i += 5)
                    {
                        string[] table = { data[i + 1], data[i + 2], data[i + 3], data[i + 4], data[i + 5] };
                        tblArcProdotti.Rows.Add(table);
                    }
                    break;
                }
            }
        }
        private void btnArcDelete_Click(object sender, EventArgs e)                                 //Rimuove dall'archivio il D.D.T. attualmente visualizzato
        {
            string[] lines = System.IO.File.ReadAllLines(archivio_csv);
            System.IO.File.WriteAllText(archivio_csv, string.Empty);
            foreach (string line in lines)
            {
                string[] data = line.Split(';');
                if (data[0].Equals(tblArchivio.Rows[tblArchivio.CurrentRow.Index].Cells[0].Value.ToString()) && data[1].Equals(tblArchivio.Rows[tblArchivio.CurrentRow.Index].Cells[1].Value.ToString()) && data[2].Equals(tblArchivio.Rows[tblArchivio.CurrentRow.Index].Cells[2].Value.ToString()) && data[5].Equals(tblArchivio.Rows[tblArchivio.CurrentRow.Index].Cells[3].Value.ToString()))
                {
                    DialogResult dialogResult = MessageBox.Show("Rimuovere definitivamente il D.D.T. n°: " + data[0] + ", generato in data " + data[2] + "?", "RIMUOVI", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                        System.IO.File.AppendAllText(archivio_csv, line + "\n");
                }
                else { System.IO.File.AppendAllText(archivio_csv, line + "\n"); }
            }
            archivioFillTable(archivio_csv);
        }
        private void txtMgzProdotto_TextChanged(object sender, EventArgs e)                         //Filtra gli elementi in tabella in base al nome prodotto
        {
            if (txtMgzProdotto.Focused)
            {
                if (!string.IsNullOrWhiteSpace(txtMgzProdotto.Text))
                {
                    string searchValue = txtMgzProdotto.Text.ToUpper();
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(magazzino_csv);
                        tblMagazzino.Rows.Clear();
                        foreach (string line in lines)
                        {
                            string[] data = line.Split(';');
                            if (data[1].ToUpper().Contains(searchValue))
                                tblMagazzino.Rows.Add(data);
                        }
                    }
                    catch (Exception exc) { MessageBox.Show(exc.Message); }
                }
                else { magazzinoFillTable(magazzino_csv); }
            }
        }
        private void txtCodiceProdotto_TextChanged(object sender, EventArgs e)                      //Autompila campi in bolla se trova prodotto uguale al codice inserito
        {
            if (txtCodiceProdotto.Focused)
            {
                if (!string.IsNullOrWhiteSpace(txtCodiceProdotto.Text))
                {
                    string[] lines = System.IO.File.ReadAllLines(magazzino_csv);
                    foreach (string line in lines)
                    {
                        string[] data = line.Split(';');
                        if (data[0].Equals(txtCodiceProdotto.Text.ToUpper()))
                        {
                            txtNomeProdotto.Text = data[1];
                            txtUnitaMisura.Text = data[3];
                            txtPrezzo.Value = Convert.ToDecimal(data[4]);
                            break;
                        }
                        else
                        {
                            txtNomeProdotto.Text = string.Empty;
                            txtUnitaMisura.Text = string.Empty;
                            txtPrezzo.Value = 0;
                        }
                    }
                }
                else { txtBollaClean(); } 
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)                       //In caso di chiusura Form ripristina nel magazzino le quantità di eventuali elementi ancora presenti in DDT
        {
            foreach (Bolla obj in bollaBindingSource)
            {
                mgzQuantitaProdotti(Convert.ToString(obj.codice.ToUpper()), false, Convert.ToInt32(obj.quantita));
            }
        }
        private void txtPercentualeIva_ValueChanged(object sender, EventArgs e)                     //Aggiorna il valore di Percentiale IVA ogni volta che viene cambiato
        {
            percentuale_iva = Convert.ToString(txtPercentualeIva.Value);
        }
        private void btnBackup_Click(object sender, EventArgs e)                                    //Genera file di backup a partire da tutti i file usati nel software
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string pathFoldersChosen = folderBrowserDialog1.SelectedPath + "\\MaFraMPL_BACKUP_" + DateTime.Today.Day.ToString("00") + DateTime.Today.Month.ToString("00") + DateTime.Today.Year.ToString() + "_" + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".txt";
                string file = System.IO.File.ReadAllText(clienti_csv) + "|\n" + System.IO.File.ReadAllText(magazzino_csv) + "|\n" + System.IO.File.ReadAllText(archivio_csv);
                System.IO.File.WriteAllText(backup_txt, file);
                System.IO.File.Copy(backup_txt, pathFoldersChosen , true);
            }
        }
        private void btnImport_Click(object sender, EventArgs e)                                    //Genera i file di sistema a partire da un file di backup importato dall'utente precedentemente generato
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path_chosen = openFileDialog1.FileName;
                if (path_chosen.Contains("MaFraMPL_BACKUP_"))
                {
                    string backup_file = System.IO.File.ReadAllText(path_chosen);
                    string[] files = backup_file.Split('|');
                    if (files.Length == 3)
                    {
                        DialogResult dialogResult = MessageBox.Show("Importando un file di backup il software tornerà allo stato in cui si trovava quando il file è stato generato.\nContinuare?", "ATTENZIONE", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            checkAppdata();
                            if (!string.IsNullOrWhiteSpace(files[0]))
                                System.IO.File.WriteAllText(clienti_csv, files[0].Trim() + "\n");
                            else { System.IO.File.WriteAllText(clienti_csv, string.Empty); }
                            if (!string.IsNullOrWhiteSpace(files[1]))
                                System.IO.File.WriteAllText(magazzino_csv, files[1].Trim() + "\n");
                            else { System.IO.File.WriteAllText(magazzino_csv, string.Empty); }
                            if (!string.IsNullOrWhiteSpace(files[2]))
                                System.IO.File.WriteAllText(archivio_csv, files[2].Trim() + "\n");
                            else { System.IO.File.WriteAllText(archivio_csv, string.Empty); }
                            clientiFillTable(clienti_csv);
                            magazzinoFillTable(magazzino_csv);
                            archivioFillTable(archivio_csv);
                        }
                    }
                    else { MessageBox.Show("Il file scelto non è valido, importare un file non valido potrebbe causare malfunzionamenti al software.", "ERRORE"); }
                }
                else { MessageBox.Show("Il file scelto non è valido, importare un file non valido potrebbe causare malfunzionamenti al software.", "ERRORE"); } 
            }
        }
        private void tblMagazzino_CellDoubleClick(object sender, DataGridViewCellEventArgs e)       //Sposta il prodotto selezionato dalla tabella magazzino alla tab "Genera DDT"
        {
            if (e.RowIndex >= 0)
            {
                txtMgzClean();
                txtBollaClean();
                txtCodiceProdotto.Text = tblMagazzino.Rows[e.RowIndex].Cells["codice"].Value.ToString();
                txtNomeProdotto.Text = tblMagazzino.Rows[e.RowIndex].Cells["descrizione"].Value.ToString();
                txtUnitaMisura.Text = tblMagazzino.Rows[e.RowIndex].Cells["unita_misura"].Value.ToString();
                txtPrezzo.Value = Convert.ToDecimal(tblMagazzino.Rows[e.RowIndex].Cells["prezzo"].Value.ToString());
                tabControl1.SelectedTab = genera_ddt;
                magazzinoFillTable(magazzino_csv);
            }
        }
        private void tblClienti_CellDoubleClick(object sender, DataGridViewCellEventArgs e)         //Trasferisce il cliente selezionato dalla tabella Clienti alla sezione Clienti della tab "Genera DDT"
        {
            if (e.RowIndex >= 0)
            {
                txtClnClean();
                txtNomeCliente.Text = tblClienti.Rows[e.RowIndex].Cells[1].Value.ToString();
                ddt_lemma_cliente = tblClienti.Rows[e.RowIndex].Cells[1].Value.ToString();
                ddt_nome_cliente = tblClienti.Rows[e.RowIndex].Cells[2].Value.ToString();
                ddt_codice_cliente = tblClienti.Rows[e.RowIndex].Cells[0].Value.ToString();
                ddt_partita_iva_cliente = tblClienti.Rows[e.RowIndex].Cells[3].Value.ToString();
                ddt_codice_fiscale_cliente = tblClienti.Rows[e.RowIndex].Cells[4].Value.ToString();
                ddt_indirizzo_cliente = tblClienti.Rows[e.RowIndex].Cells[5].Value.ToString();
                ddt_citta_cliente = tblClienti.Rows[e.RowIndex].Cells[6].Value.ToString();
                ddt_destinazione_cliente = tblClienti.Rows[e.RowIndex].Cells[7].Value.ToString();
                tabControl1.SelectedTab = genera_ddt;
                clientiFillTable(clienti_csv);
            }
        }
        private void mgzQuantitaProdotti(string codice_prodotto, bool aggiungi, int quantità)       //Modifica quantità nel magazzino in caso di prodotto aggiunto in bolla o rimosso
        {
            string[] rows = System.IO.File.ReadAllLines(magazzino_csv);
            System.IO.File.WriteAllText(magazzino_csv, string.Empty);
            foreach (string row in rows)
            {
                string[] data = row.Split(';');
                if (data[0].Equals(codice_prodotto.ToUpper()))
                {
                    string new_row = aggiungi ? $"{data[0]};{data[1]};{(Convert.ToInt32(data[2]) - quantità).ToString()};{data[3]};{data[4]};{data[5]}" : $"{data[0]};{data[1]};{(Convert.ToInt32(data[2]) + quantità).ToString()};{data[3]};{data[4]};{data[5]}";
                    System.IO.File.AppendAllText(magazzino_csv, new_row + "\n");
                }
                else { System.IO.File.AppendAllText(magazzino_csv, row + "\n"); }
            }
            magazzinoFillTable(magazzino_csv);
        }
    }
}