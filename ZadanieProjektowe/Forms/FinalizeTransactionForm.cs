﻿using System;
using System.Linq;
using System.Windows.Forms;
using PubSub;
using ZadanieProjektowe.Classes;
using ZadanieProjektowe.Classes.Events;

namespace ZadanieProjektowe.Forms
{
    public partial class FinalizeTransactionForm : Form
    {
        private readonly Transaction _transaction;

        public event Action<Transaction> Save;

        public FinalizeTransactionForm(Transaction transaction)
        {
            _transaction = transaction;
            InitializeComponent();
        }

        private void FinalizeTransactionForm_Load(object sender, EventArgs e)
        {
            label1.Text = _transaction.Sum.ToString("C");

            var db = new Entities();
            listBox1.DataSource = db.Customers.ToList();
            listBox1.DisplayMember = "Name";
            listBox1.Invalidate();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var db = new Entities();

            var customer = ((Customer)listBox1.SelectedItem);

            var invoice = new Invoice
            {
                CustomerId = customer.Id,
                Date = DateTime.Now,
                Amount = _transaction.Sum
            };
            db.Invoices.Add(invoice);

            foreach (var transactionItem in _transaction.Items)
            {
                var position = new InvoicesPosition
                {
                    InvoiceId = invoice.Id,
                    ProductId = transactionItem.Product.Id,
                    Price = transactionItem.Product.Price,
                    Quanity = (short) transactionItem.Quanity

                };
                db.InvoicesPositions.Add(position);
                var product = db.Products.First(p => p.Id == transactionItem.Product.Id);
                product.Quanity -= (short) transactionItem.Quanity;
            }

            db.SaveChanges();
            
            OnSave(invoice);
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected virtual void OnSave(Invoice invoice)
        {
            Save?.Invoke(_transaction);
            this.Publish(new NewInvoiceWasCreatedEvent(invoice));
        }
    }
}
