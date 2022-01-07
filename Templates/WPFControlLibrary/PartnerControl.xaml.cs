using System;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;
$if$ ($targetframeworkversion$ >= 4.5)using System.Threading.Tasks;
$endif$using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using %RootFormName%.DAL;
using %RootFormName%.DAL.Classes;

namespace $safeprojectname$
{
    #region Database WPF data model with example fields

    public class DataModel : INotifyPropertyChanged
    {
        private DataAccessLayer db { get; set; }

        public DataModel()
        {
        }

        public DataModel(DataAccessLayer db, string entityRef, int matterNo)
        {
            this.db = db;
        }

        public void Save()
        {
        }

        // Demo, can be removed when building the DataModel object
        private string _StringField;
        public string StringField
        {
            get
            {
                return (_StringField);
            }
            set
            {
                _StringField = value;
                RaisePropertyChanged("StringField");
            }
        }

        // Demo, can be removed when building the DataModel object
        private int _IntegerField;
        public int IntegerField
        {
            get
            {
                return (_IntegerField);
            }
            set
            {
                _IntegerField = value;
                RaisePropertyChanged("IntegerField");
            }
        }

        // Demo, can be removed when building the DataModel object
        private bool _BooleanField;
        public bool BooleanField
        {
            get
            {
                return (_BooleanField);
            }
            set
            {
                _BooleanField = value;
                RaisePropertyChanged("BooleanField");
            }
        }

        // Demo, can be removed when building the DataModel object
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    #endregion

    /// <summary>
    /// Interaction logic for $safeitemrootname$.xaml
    /// </summary>
    public partial class PartnerControl : Grid
    {
        private DataAccessLayer _dataLayer;

        public DataModel _dataModel { get; set; } = new DataModel();

        private int _btnCount { get; set; } = 0;

        private string _dbConnect { get; set; } = string.Empty;
        private string _entityRef { get; set; }
        private int    _matterNo  { get; set; }
        private string _userCode  { get; set; }
        private DependencyObject _parentView { get; set; }

        public PartnerControl()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                this.DataContext = _dataModel;
            };
        }

        public PartnerControl(
            string dbString, string entityRef, int matterNo, string userCode,
            DependencyObject parentView)
        {
            InitializeComponent();

            _dbConnect = dbString;
            _entityRef = entityRef;
            _matterNo  = matterNo;
            _userCode  = userCode;
            _parentView = parentView;

            _dataLayer = new DataAccessLayer(dbString);

            Loaded += (sender, args) =>
            {
                _dataModel = new DataModel(_dataLayer, entityRef, matterNo);
                this.DataContext = _dataModel;
            };
        }

        // Allows the hosting binary to correctly initialize the DB DAL while outside [Partner]
        public void SetupDataLayer(string dbString, string entityRef, int matterNo)
        {
            _dbConnect = dbString;
            _dataLayer = new DataAccessLayer(dbString);
            _dataModel = new DataModel(_dataLayer, entityRef, matterNo);
            this.DataContext = _dataModel;
        }

        private DependencyObject FindNode(string elementName, DependencyObject logicalTreeNode = null)
            => LogicalTreeHelper.FindLogicalNode(logicalTreeNode ?? _parentView, elementName);

        private T FindNode<T>(string elementName, DependencyObject logicalTreeNode = null)
            => (T)(object)FindNode(elementName, logicalTreeNode);

        private void btnTestClick(object sender, RoutedEventArgs e)
        {
            if (_dataModel != null)
            {
                _btnCount++;
                _dataModel.StringField = string.Format("Test Button Clicked: {0}", _btnCount);
            }
        }

        private void btnCloseClick(object sender, RoutedEventArgs e)
        {
            var cancelButton = FindNode<Button>("Cancel");

            cancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, cancelButton));
        }
    }
}
