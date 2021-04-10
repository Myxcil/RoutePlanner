
namespace RoutePlanner
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonNewRoute = new System.Windows.Forms.Button();
            this.checkedListBoxRoutes = new System.Windows.Forms.CheckedListBox();
            this.labelFrom = new System.Windows.Forms.Label();
            this.comboBoxFrom = new System.Windows.Forms.ComboBox();
            this.comboBoxTo = new System.Windows.Forms.ComboBox();
            this.labelTo = new System.Windows.Forms.Label();
            this.labelCargo = new System.Windows.Forms.Label();
            this.numericUpDownCargo = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPassenger = new System.Windows.Forms.NumericUpDown();
            this.labelPassenger = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.groupBoxCreateRoute = new System.Windows.Forms.GroupBox();
            this.labelTotalDistance = new System.Windows.Forms.Label();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.listBoxSearch = new System.Windows.Forms.ListBox();
            this.labelTotalSearch = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCargo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPassenger)).BeginInit();
            this.groupBoxCreateRoute.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonNewRoute
            // 
            this.buttonNewRoute.Location = new System.Drawing.Point(13, 13);
            this.buttonNewRoute.Name = "buttonNewRoute";
            this.buttonNewRoute.Size = new System.Drawing.Size(75, 23);
            this.buttonNewRoute.TabIndex = 0;
            this.buttonNewRoute.TabStop = false;
            this.buttonNewRoute.Text = "New Route";
            this.buttonNewRoute.UseVisualStyleBackColor = true;
            this.buttonNewRoute.Click += new System.EventHandler(this.btnNewRoute_Click);
            // 
            // checkedListBoxRoutes
            // 
            this.checkedListBoxRoutes.FormattingEnabled = true;
            this.checkedListBoxRoutes.Location = new System.Drawing.Point(287, 13);
            this.checkedListBoxRoutes.Name = "checkedListBoxRoutes";
            this.checkedListBoxRoutes.Size = new System.Drawing.Size(228, 124);
            this.checkedListBoxRoutes.TabIndex = 1;
            this.checkedListBoxRoutes.TabStop = false;
            // 
            // labelFrom
            // 
            this.labelFrom.AutoSize = true;
            this.labelFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFrom.Location = new System.Drawing.Point(6, 16);
            this.labelFrom.Name = "labelFrom";
            this.labelFrom.Size = new System.Drawing.Size(34, 13);
            this.labelFrom.TabIndex = 3;
            this.labelFrom.Text = "From";
            // 
            // comboBoxFrom
            // 
            this.comboBoxFrom.FormattingEnabled = true;
            this.comboBoxFrom.Location = new System.Drawing.Point(9, 33);
            this.comboBoxFrom.Name = "comboBoxFrom";
            this.comboBoxFrom.Size = new System.Drawing.Size(67, 21);
            this.comboBoxFrom.TabIndex = 0;
            this.comboBoxFrom.Leave += new System.EventHandler(this.comboBoxFrom_Leave);
            // 
            // comboBoxTo
            // 
            this.comboBoxTo.FormattingEnabled = true;
            this.comboBoxTo.Location = new System.Drawing.Point(82, 33);
            this.comboBoxTo.Name = "comboBoxTo";
            this.comboBoxTo.Size = new System.Drawing.Size(67, 21);
            this.comboBoxTo.TabIndex = 1;
            this.comboBoxTo.Leave += new System.EventHandler(this.comboBoxTo_Leave);
            // 
            // labelTo
            // 
            this.labelTo.AutoSize = true;
            this.labelTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTo.Location = new System.Drawing.Point(79, 16);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(22, 13);
            this.labelTo.TabIndex = 5;
            this.labelTo.Text = "To";
            // 
            // labelCargo
            // 
            this.labelCargo.AutoSize = true;
            this.labelCargo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCargo.Location = new System.Drawing.Point(6, 57);
            this.labelCargo.Name = "labelCargo";
            this.labelCargo.Size = new System.Drawing.Size(40, 13);
            this.labelCargo.TabIndex = 7;
            this.labelCargo.Text = "Cargo";
            // 
            // numericUpDownCargo
            // 
            this.numericUpDownCargo.Location = new System.Drawing.Point(9, 74);
            this.numericUpDownCargo.Name = "numericUpDownCargo";
            this.numericUpDownCargo.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownCargo.TabIndex = 2;
            // 
            // numericUpDownPassenger
            // 
            this.numericUpDownPassenger.Location = new System.Drawing.Point(138, 74);
            this.numericUpDownPassenger.Name = "numericUpDownPassenger";
            this.numericUpDownPassenger.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownPassenger.TabIndex = 3;
            // 
            // labelPassenger
            // 
            this.labelPassenger.AutoSize = true;
            this.labelPassenger.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPassenger.Location = new System.Drawing.Point(135, 57);
            this.labelPassenger.Name = "labelPassenger";
            this.labelPassenger.Size = new System.Drawing.Size(66, 13);
            this.labelPassenger.TabIndex = 9;
            this.labelPassenger.Text = "Passenger";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(155, 31);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(103, 23);
            this.buttonAdd.TabIndex = 4;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // groupBoxCreateRoute
            // 
            this.groupBoxCreateRoute.Controls.Add(this.labelFrom);
            this.groupBoxCreateRoute.Controls.Add(this.buttonAdd);
            this.groupBoxCreateRoute.Controls.Add(this.comboBoxFrom);
            this.groupBoxCreateRoute.Controls.Add(this.numericUpDownPassenger);
            this.groupBoxCreateRoute.Controls.Add(this.labelTo);
            this.groupBoxCreateRoute.Controls.Add(this.labelPassenger);
            this.groupBoxCreateRoute.Controls.Add(this.comboBoxTo);
            this.groupBoxCreateRoute.Controls.Add(this.numericUpDownCargo);
            this.groupBoxCreateRoute.Controls.Add(this.labelCargo);
            this.groupBoxCreateRoute.Location = new System.Drawing.Point(12, 42);
            this.groupBoxCreateRoute.Name = "groupBoxCreateRoute";
            this.groupBoxCreateRoute.Size = new System.Drawing.Size(269, 110);
            this.groupBoxCreateRoute.TabIndex = 12;
            this.groupBoxCreateRoute.TabStop = false;
            this.groupBoxCreateRoute.Text = "Route";
            // 
            // labelTotalDistance
            // 
            this.labelTotalDistance.AutoSize = true;
            this.labelTotalDistance.Location = new System.Drawing.Point(288, 138);
            this.labelTotalDistance.Name = "labelTotalDistance";
            this.labelTotalDistance.Size = new System.Drawing.Size(31, 13);
            this.labelTotalDistance.TabIndex = 13;
            this.labelTotalDistance.Text = "Total";
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(13, 159);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(100, 23);
            this.buttonSearch.TabIndex = 14;
            this.buttonSearch.Text = "Search Route";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // listBoxSearch
            // 
            this.listBoxSearch.FormattingEnabled = true;
            this.listBoxSearch.Location = new System.Drawing.Point(13, 189);
            this.listBoxSearch.Name = "listBoxSearch";
            this.listBoxSearch.Size = new System.Drawing.Size(268, 238);
            this.listBoxSearch.TabIndex = 15;
            // 
            // labelTotalSearch
            // 
            this.labelTotalSearch.AutoSize = true;
            this.labelTotalSearch.Location = new System.Drawing.Point(9, 430);
            this.labelTotalSearch.Name = "labelTotalSearch";
            this.labelTotalSearch.Size = new System.Drawing.Size(31, 13);
            this.labelTotalSearch.TabIndex = 16;
            this.labelTotalSearch.Text = "Total";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 540);
            this.Controls.Add(this.labelTotalSearch);
            this.Controls.Add(this.listBoxSearch);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.labelTotalDistance);
            this.Controls.Add(this.groupBoxCreateRoute);
            this.Controls.Add(this.checkedListBoxRoutes);
            this.Controls.Add(this.buttonNewRoute);
            this.Name = "MainForm";
            this.Text = "Route Planner for OnAir";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCargo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPassenger)).EndInit();
            this.groupBoxCreateRoute.ResumeLayout(false);
            this.groupBoxCreateRoute.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonNewRoute;
        private System.Windows.Forms.CheckedListBox checkedListBoxRoutes;
        private System.Windows.Forms.Label labelFrom;
        private System.Windows.Forms.ComboBox comboBoxFrom;
        private System.Windows.Forms.ComboBox comboBoxTo;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.Label labelCargo;
        private System.Windows.Forms.NumericUpDown numericUpDownCargo;
        private System.Windows.Forms.NumericUpDown numericUpDownPassenger;
        private System.Windows.Forms.Label labelPassenger;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.GroupBox groupBoxCreateRoute;
        private System.Windows.Forms.Label labelTotalDistance;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.ListBox listBoxSearch;
        private System.Windows.Forms.Label labelTotalSearch;
    }
}

