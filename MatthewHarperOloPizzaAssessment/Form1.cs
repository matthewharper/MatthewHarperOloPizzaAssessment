using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MatthewHarperOloPizzaAssessment
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.pizzaGrid.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.pizzaGrid_RowPostPaint);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                status.Text = "... getting order data." ;
                status.Refresh();
                showTop20OrderDetails();
                status.Text = "... showing order details.";
                status.Refresh();
            }
            catch (Exception ex)
            {
                status.Text = "Error!";
                //mjh:  Only performing elementary error handling for Assessment.
                MessageBox.Show(@"An unexpected error occurred accessing or processing the JSON data.

Please check that the data exists and that it is formatted correctly.

Error details are below: 

" + ex.ToString());
            }
        }

        //mjh:  In a real project, this would be broken out appropriately.  
        //      Keeping all logic here so it is easier for you to read.
        //      Would put URL to data in config or db in real code.
        private void showTop20OrderDetails()
        {

            string json;

            // Get the data from the endpoint.
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString(dataUrlTextBox.Text);
            }
            
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(List<Pizza>));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var orderHistory = (List<Pizza>)dcjs.ReadObject(ms);

            PizzaComparer pc = new PizzaComparer();

            // Put the data in the correct format.
            var g = orderHistory
                .GroupBy(p => p, pc)
                .Select(p => new
                {
                    ConfigurationID = p.Key.GetHashCode(),
                    ConfigurationCount = p.Count(),
                    ConfigurationToppings = string.Join(",", p.Key.toppings)
                }

                )
                .OrderByDescending(p => p.ConfigurationCount)
                .Take(20);

            // Show your stuff.
            pizzaGrid.DataSource = g.ToList();
            
        }

        // I didn't realize line numbers were not native to the DataGridView control.
        // I do more programming in the web world, but this routine is pretty solid.
        private void pizzaGrid_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(pizzaGrid.RowHeadersDefaultCellStyle.ForeColor))
            {
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 10, e.RowBounds.Location.Y + 4);
            }
        }
    }

    // Something to work with.
    [DataContract]
    public class Pizza
    {
        [DataMember]
        public string[] toppings { get; set; }
    }


    // Ensure we are comparing values for equals and not the object reference.
    public class PizzaComparer : IEqualityComparer<Pizza>
    {
        public bool Equals(Pizza x, Pizza y)
        {
            return x.toppings.SequenceEqual(y.toppings);
        }

        public int GetHashCode(Pizza obj)
        {
            //mjh: Hack to generate unique id.  This may break if the order is different but the values are the same.
            //      (i.e. {"pepperoni", "mushroom"} should equal {"mushroom", "pepperoni"})
            //      I'm a little short on time to test.  SequenceEqual may have taken care of that since it compares
            //      the values of the data instead of an object compare which would be a reference compare.
            return (String.Join("|", obj.toppings)).GetHashCode();
        }
    }
}




