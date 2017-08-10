﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Molmed.ChiasmaDep.Data;
using Molmed.ChiasmaDep.Data.Exception;

namespace Molmed.ChiasmaDep.Dialog
{
    public partial class SampleListDialog : Form
    {
        private IGenericContainer MyPutInContainer;
        private BarCodeController MyBarCodeController;
        public SampleListDialog()
        {
            InitializeComponent();
            MyPutInContainer = null;
            MyBarCodeController = null;
            InitListView();
        }

        public SampleListDialog(IGenericContainer container)
        {
            InitializeComponent();
            InitListView();
            MyBarCodeController = new BarCodeController(this);
            MyBarCodeController.BarCodeReceived += new BarCodeEventHandler(BarCodeReceived);
            if (LoadSampleStorageDuoDialog.IsSampleContainer(container))
            {
                InitWithSampleContainer(container);
            }
            else if (LoadSampleStorageDuoDialog.IsStorageContainer(container))
            {
                InitWithPutInContainer(container);
            }
            else
            {
                throw new Data.Exception.DataException("This container neither represent a sample container nor a deposit");            
            }
        }

        private void HandleReceivedBarCode(String barCode)
        {
            IGenericContainer container;
            container = GenericContainerManager.GetGenericContainerByBarCode(barCode);
            if (LoadSampleStorageDuoDialog.IsSampleContainer(container))
            {
                SampleContainerListView.Items.Add(new ContainerToBePlacedViewItem(container));
                if (MyPutInContainer != null)
                {
                    OkButton.Enabled = true;
                }
            }
            else if (LoadSampleStorageDuoDialog.IsStorageContainer(container))
            {
                MyPutInContainer = container;
                PutInContainerTextBox.Text = container.GetIdentifier();
                DialogResult = DialogResult.OK;
            }
            else
            {
                throw new Data.Exception.DataException("This container neither represent a sample container nor a deposit");
            }        
        }

        private void BarCodeReceived(String barCode)
        {
            try
            {
                HandleReceivedBarCode(barCode);
            }
            catch (BarCodeException ex)
            {
                ShowWarning(ex.Message);
            }
            catch (Exception ex)
            {
                LoadSampleStorageDuoDialog.HandleError(ex.Message, ex);
            }
            
        }


        protected void ShowWarning(String message)
        {
            MessageBox.Show(message,
                           Config.GetDialogTitleStandard(),
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Exclamation);
        }


        private void InitWithSampleContainer(IGenericContainer container)
        {
            SampleContainerListView.BeginUpdate();
            SampleContainerListView.Items.Add(new ContainerToBePlacedViewItem(container));
            SampleContainerListView.EndUpdate();
            MyPutInContainer = null;
            StatusLabel.Text = "Waiting for more scanned containers (plates/tubes/etc). \nClose and go to next step by scan a deposit (Box/Shelf/Freezer/etc)";
            OkButton.Enabled = false;
        }

        public IGenericContainer GetSelectedDeposit()
        {
            return MyPutInContainer;
        }

        public GenericContainerList GetSelectedContainers()
        {
            GenericContainerList containers = new GenericContainerList();
            foreach (ContainerToBePlacedViewItem viewItem in SampleContainerListView.Items)
            {
                containers.Add(viewItem.GetContainerToBePlaced());
            }
            return containers;
        }

        private void InitWithPutInContainer(IGenericContainer container)
        {
            StatusLabel.Text = "Waiting for scanned containers (plates/tubes/etc) to be placed in the deposit below. \nClose and go to next step by pressing OK or cancel.";
            PutInContainerTextBox.Text = container.GetIdentifier();
            MyPutInContainer = container;
            OkButton.Enabled = false;
        }

        private void InitListView()
        {
            SampleContainerListView.Columns.Add("Container to be placed", -2);
        }

        private class ContainerToBePlacedViewItem : ListViewItem
        {
            private IGenericContainer MyContainer;

            public ContainerToBePlacedViewItem(IGenericContainer container)
                : base(container.GetIdentifier())
            {
                MyContainer = container;
            }

            public IGenericContainer GetContainerToBePlaced()
            {
                return MyContainer;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void MyCancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}