using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ma_Fra_MPL
{
    public partial class LayoutBolla : Form
    {
        List<Bolla> _list;
        string _nome_cliente, _lemma_cliente, _indirizzo_cliente, _citta_cliente, _destinazione_cliente, _numero_documento, _data_documento, _aspetto_esteriore_beni, _causale_trasporto, _numero_colli, _codice_cliente, _partita_iva, _codice_fiscale, _data_inizio_trasporto, _ora_inizio_trasporto, _percentuale_iva, _imponibile, _imposta, _totale_documento;

        string _archivio_file_path;
        bool _senza_prezzo = false;
        public LayoutBolla(List<Bolla> dataSource, string nome_cliente, string lemma_cliente, string indirizzo_cliente, string citta_cliente, string destinazione_cliente, string numero_documento, string data_documento, string aspetto_esteriore_beni, string causale_trasporto, string numero_colli, string codice_cliente, string partita_iva, string codice_fiscale, string data_inizio_trasporto, string ora_inizio_trasporto, string percentuale_iva, string imponibile, string imposta, string totale_documento, bool senza_prezzo, string archivio_file_path)
        {
            InitializeComponent();
            _list = dataSource;
            _nome_cliente = nome_cliente;
            _indirizzo_cliente = indirizzo_cliente;
            _citta_cliente = citta_cliente;
            _destinazione_cliente = destinazione_cliente;
            _numero_documento = numero_documento;
            _data_documento = data_documento;
            _aspetto_esteriore_beni = aspetto_esteriore_beni;
            _causale_trasporto = causale_trasporto;
            _numero_colli = numero_colli;
            _codice_cliente = codice_cliente;
            _partita_iva = partita_iva;
            _codice_fiscale = codice_fiscale;
            _data_inizio_trasporto = data_inizio_trasporto;
            _ora_inizio_trasporto = ora_inizio_trasporto;
            _percentuale_iva = percentuale_iva;
            _imponibile = imponibile;
            _imposta = imposta;
            _totale_documento = totale_documento;
            
            _lemma_cliente = lemma_cliente;
            _senza_prezzo = senza_prezzo;
            _archivio_file_path = archivio_file_path;
        }
        private void LayoutBolla_Load(object sender, EventArgs e)
        {
            ReportDataSource source = new ReportDataSource("DataSet1", _list);
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(source);
            reportViewer.LocalReport.ReportPath = _senza_prezzo ? @"LayoutSenzaPrezzo.rdlc" : @"Layout1.rdlc";
            Microsoft.Reporting.WinForms.ReportParameter[] parameters = new Microsoft.Reporting.WinForms.ReportParameter[]
            {
                new Microsoft.Reporting.WinForms.ReportParameter("pNomeCliente", _nome_cliente),
                new Microsoft.Reporting.WinForms.ReportParameter("pIndirizzoCliente", _indirizzo_cliente),
                new Microsoft.Reporting.WinForms.ReportParameter("pCittaCliente", _citta_cliente),
                new Microsoft.Reporting.WinForms.ReportParameter("pDestinazioneCliente", _destinazione_cliente),
                new Microsoft.Reporting.WinForms.ReportParameter("pNumeroDocumento", _numero_documento),
                new Microsoft.Reporting.WinForms.ReportParameter("pDataDocumento", _data_documento),
                new Microsoft.Reporting.WinForms.ReportParameter("pAspettoEsterioreBeni", _aspetto_esteriore_beni),
                new Microsoft.Reporting.WinForms.ReportParameter("pCausaleTrasporto", _causale_trasporto),
                new Microsoft.Reporting.WinForms.ReportParameter("pNumeroColli", _numero_colli),
                new Microsoft.Reporting.WinForms.ReportParameter("pCodiceCliente", _codice_cliente),
                new Microsoft.Reporting.WinForms.ReportParameter("pPartitaIva", _partita_iva),
                new Microsoft.Reporting.WinForms.ReportParameter("pCodiceFiscale", _codice_fiscale),
                new Microsoft.Reporting.WinForms.ReportParameter("pDataInizioTrasporto", _data_inizio_trasporto),
                new Microsoft.Reporting.WinForms.ReportParameter("pOraInizioTrasporto", _ora_inizio_trasporto),
                new Microsoft.Reporting.WinForms.ReportParameter("pPercentualeIva", _percentuale_iva),
                new Microsoft.Reporting.WinForms.ReportParameter("pImponibile", _imponibile),
                new Microsoft.Reporting.WinForms.ReportParameter("pImposta", _imposta),
                new Microsoft.Reporting.WinForms.ReportParameter("pTotaleDocumento", _totale_documento)
            };
            this.reportViewer.LocalReport.SetParameters(parameters);
            this.reportViewer.RefreshReport();
            archiviaBolla();
        }
        private void archiviaBolla()
        {
            string da_archiviare = $"{_numero_documento};{_lemma_cliente};{_data_documento};{_imponibile};{_imposta};{_totale_documento};{_percentuale_iva};{_list.Count};";
            foreach (var element in _list)
            {
                da_archiviare += ($"{element.codice};{element.descrizione};{element.quantita};{element.prezzo};{element.importo};");
            }
            System.IO.File.AppendAllText(_archivio_file_path, da_archiviare.TrimEnd(';') + "\n");
        }
        private void reportViewer_PrintingBegin(object sender, ReportPrintEventArgs e)
        {
            this.Close();
        }
    }
}
