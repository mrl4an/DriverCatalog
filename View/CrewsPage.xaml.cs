using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DriverCatalog.View
{
    public class CrewsModel : INotifyPropertyChanged
    {
        private int id;
        private int carId;
        private int driverId;
        private bool isDelete;
        public int Id
        {
            get { return id; }
            set
            {
                this.id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public int CarId
        {
            get { return carId; }
            set
            {
                this.carId = value;
                OnPropertyChanged(nameof(CarId));
            }
        }

        public int DriverId
        {
            get { return driverId; }
            set
            {
                this.driverId = value;
                OnPropertyChanged(nameof(DriverId));
            }
        }

        public bool IsDelete
        {
            get { return isDelete; }
            set
            {
                this.isDelete = value;
                OnPropertyChanged(nameof(IsDelete));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// Логика взаимодействия для CrewsPage.xaml
    /// </summary>
    public partial class CrewsPage : Page
    {
        private BDManager manager;
        public ObservableCollection<CrewsModel> Crews { get; set; }

        public ObservableCollection<CrewsModel> FilteredCrews { get; set; }
        public CrewsPage(BDManager manager)
        {
            InitializeComponent();
            DataContext = this;
            this.manager = manager;
            LoadData();
        }
        public void setFilter()
        {
            if (FilteredCrews == null)
                FilteredCrews = new ObservableCollection<CrewsModel>();

            FilteredCrews.Clear();
            foreach (var car in Crews)
            {
                if (!car.IsDelete)
                    FilteredCrews.Add(car);
            }
        }

        private void LoadData()
        {
            Crews = new ObservableCollection<CrewsModel>();
            foreach (var crew in manager.GetAllCrews())
                Crews.Add(new CrewsModel() { Id = crew.Id, CarId = crew.CarId, DriverId = crew.DriverId });
            setFilter();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CrewsModel newCrew= new CrewsModel();
            Crews.Add(newCrew);
            setFilter();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                var selectedCar = (CrewsModel)dataGrid.SelectedItem;
                selectedCar.IsDelete = true;
            }
            setFilter();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (check())
                return;
            foreach (var crew in Crews)
            {
                if (crew.IsDelete)
                {
                    if (manager.GetCrewById(crew.Id) != null)
                        manager.DeleteCrew(crew.Id);
                    continue;
                }
                Crew curCrew = new Crew()
                {
                    Id = crew.Id,
                    CarId = crew.CarId,
                    DriverId = crew.DriverId
                };

                try
                {
                    manager.UpdateCrew(curCrew);
                }
                catch (Exception ex)
                {
                    manager.AddCrew(curCrew);
                }
            }
            LoadData();
        }

        private bool check()
        {
            bool err = false;
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);

                if (row != null)
                {
                    row.Background = Brushes.White;
                }
            }
            List<object> ints = new List<object>();
            foreach (var item in dataGrid.Items)
            {
                var itemType = item.GetType();

                var idProperty = itemType.GetProperty("Id");

                if (idProperty != null)
                {
                    var itemId = idProperty.GetValue(item);

                    if (itemId != null && ints.Contains(itemId))
                    {
                        var dataGridRow = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                        if (dataGridRow != null)
                        {
                            dataGridRow.Background = Brushes.Red;
                        }
                        err = true;
                    }
                    else
                    {
                        ints.Add(itemId);
                    }
                }

                idProperty = itemType.GetProperty("CarId");
                if (idProperty != null)
                {
                    int carId = (int)idProperty.GetValue(item);
                    if (manager.GetCarById((int)carId) == null)
                    {
                        var dataGridRow = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                        if (dataGridRow != null)
                        {
                            dataGridRow.Background = Brushes.Red;
                        }
                        MessageBox.Show("Автомобиль с указанным ID не существует.");
                        return true;
                    }
                }

                idProperty = itemType.GetProperty("DriverId");
                if (idProperty != null)
                {
                    int driverId = (int)idProperty.GetValue(item);
                    if (manager.GetDriverById((int)driverId) == null)
                    {
                        var dataGridRow = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                        if (dataGridRow != null)
                        {
                            dataGridRow.Background = Brushes.Red;
                        }
                        MessageBox.Show("Экипаж с указанным ID уже существует");
                        return true;
                    }
                }
            }

            if (err)
                MessageBox.Show("Красный цветом выделены строки у которых повторяется id");
            else
                MessageBox.Show("Данные обновлены");

            return err;
        }
    }
}
