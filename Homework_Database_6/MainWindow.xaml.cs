using Homework_Database_6;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Security.Policy;
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
using static System.Net.Mime.MediaTypeNames;

namespace Homework_6
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadData();  // загрузка данных в комбобоксы (студии и страны)
                         // и таблицу datagrid (студии)
        }


        private void LoadData()
        {
            using (GamesEntities db = new GamesEntities())
            {
                StudiosDataGrid.ItemsSource = db.Studios.ToList();
                dataGrid.ItemsSource = db.Games.ToList();

                // Комбобокс со странами
                CountryComboBox.ItemsSource = db.Countries.ToList();
                CountryComboBox.DisplayMemberPath = "CountryName"; // Отображение имени
                CountryComboBox.SelectedValuePath = "ID";  // Хранение Id

                // Комбобокс со студиями
                TypeStudioComboBox.ItemsSource = db.Studios.ToList();
                TypeStudioComboBox.DisplayMemberPath = "Name"; // Отображение имени
                TypeStudioComboBox.SelectedValuePath = "ID";  // Хранение Id

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /* SELECT COUNT(*) AS SinglePlayerGamesCount
               FROM Games
               WHERE IsSinglePlayer = 1;
            */
            using (GamesEntities db = new GamesEntities())
            {
                var singlePlayerGamesCount = db.Games.Count(game => game.IsSinglePlayer == true);
                dataGrid.ItemsSource = new[] { new { Count = singlePlayerGamesCount } };

            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                var MultiPlayerGamesCount = db.Games.Count(game => game.IsSinglePlayer == false);
                dataGrid.ItemsSource = new[] { new { Count = MultiPlayerGamesCount } };

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                string genre = textBox.Text;
                if (string.IsNullOrEmpty(genre)) genre = "Action-Adventure";


                dataGrid.ItemsSource = new[] {
                         db.Games
                        .Where(game => game.Genre == genre)
                        .OrderByDescending(game => game.Sales)
                        .FirstOrDefault() };


            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Топ 5 самых продаваемых игр конкретного стиля;

            using (GamesEntities db = new GamesEntities())
            {
                string genre = textBox.Text;
                if (string.IsNullOrEmpty(genre)) genre = "Action-Adventure";

                dataGrid.ItemsSource = new[] {
                         db.Games
                        .Where(game => game.Genre == genre)
                        .OrderByDescending(game => game.Sales)
                        .Take(5) };
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //Топ 5 самых непродаваемых игр конкретного стиля;

            using (GamesEntities db = new GamesEntities())
            {
                string genre = textBox.Text;
                if (string.IsNullOrEmpty(genre)) genre = "Action-Adventure";

                dataGrid.ItemsSource = new[] {
                         db.Games
                        .Where(game => game.Genre == genre)
                        .OrderBy(game => game.Sales)
                        .Take(5) };
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            // Показать все данные по играм
            using (GamesEntities db = new GamesEntities())
            {
                dataGrid.ItemsSource = db.Games.ToList();
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            //Отобразить название каждой студии и стиль,
            //по которому у студии максимальное количество игр
            using (GamesEntities db = new GamesEntities())
            {
                dataGrid.ItemsSource = db.Studios
                                        .Select(s => new
                                        {
                                            StudioName = s.Name,
                                            Genre = s.Games
                                                .GroupBy(g => g.Genre)
                                                .Select(g => new
                                                {
                                                    Genre = g.Key,
                                                    GameCount = g.Count()
                                                })
                                                .OrderByDescending(g => g.GameCount)
                                                .FirstOrDefault()
                                        })
                                        .Select(s => new
                                        {
                                            s.StudioName,
                                            Genre = s.Genre.Genre,
                                            MaxGameCount = s.Genre.GameCount
                                        })
                                        .ToList();
            }
        }


        // Получить выбранную игру из datagrid
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // Если игра выбрана
            Games selectedGame = (Games)dataGrid.SelectedItem;

            // Если игра выбрана
            if (selectedGame != null)
            {
                // Вывести информацию об игре
                GameIdTextBox.Text = selectedGame.ID.ToString();
                GameTitleTextBox.Text = selectedGame.Title;
                GameGenreTextBox.Text = selectedGame.Genre;
                GameSalesTextBox.Text = selectedGame.Sales.ToString();
                GameIsSinglePlayer.IsChecked = selectedGame.IsSinglePlayer;
                TypeStudioComboBox.SelectedValue = selectedGame.ID;
            }
        }





        private void UpdateStudioButton_Click(object sender, RoutedEventArgs e)
        {
            // обновление данных по выбранной студии
            using (GamesEntities db = new GamesEntities())
            {
                int studioId = int.Parse(StudioIdTextBox.Text);


                var sel_studio = db.Studios.SingleOrDefault(x => x.ID == studioId);

                if (sel_studio != null)
                {
                    sel_studio.Name = StudioNameTextBox.Text;
                    db.SaveChanges();
                }

                // Обновляем данные в StudiosDataGrid
                StudiosDataGrid.ItemsSource = db.Studios.ToList();


                // Обновление таблицы студия - страна
                var sel_studio_country = db.StudioCountry.SingleOrDefault(x => x.StudioID == sel_studio.ID);
                if (sel_studio_country.CountryID != (int)CountryComboBox.SelectedValue)
                {
                    sel_studio_country.CountryID = (int)CountryComboBox.SelectedValue;
                    db.SaveChanges();
                }

            }
        }


        private void DeleteStudioButton_Click(object sender, RoutedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                int studioId = int.Parse(StudioIdTextBox.Text);
                var sel_studio = db.Studios.SingleOrDefault(x => x.ID == studioId);
                if (sel_studio != null)
                {
                    if (MessageBox.Show("Точно удалить?", "Подтверждение", MessageBoxButton.OKCancel,
                        MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        db.Studios.Remove(sel_studio);
                        db.SaveChanges();
                    }
                    // Обновляем данные в StudiosDataGrid
                    StudiosDataGrid.ItemsSource = db.Studios.ToList();
                }
            }
        }

        private void UpdateGameButton_Click(object sender, RoutedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                int gameId = int.Parse(GameIdTextBox.Text);
                var sel_game = db.Games.SingleOrDefault(x => x.ID == gameId);
                if (sel_game != null)
                {
                    sel_game.Title = GameTitleTextBox.Text;
                    sel_game.Genre = GameGenreTextBox.Text;
                    sel_game.Sales = int.Parse(GameSalesTextBox.Text);
                    sel_game.IsSinglePlayer = (bool)GameIsSinglePlayer.IsChecked;
                    sel_game.StudioID = (int)TypeStudioComboBox.SelectedValue;
                    db.SaveChanges();
                }
                // Обновляем данные в dataGrid
                dataGrid.ItemsSource = db.Games.ToList();
            }
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                Games newGame = new Games
                {
                    Title = GameTitleTextBox.Text,
                    Genre = GameGenreTextBox.Text,
                    Sales = int.Parse(GameSalesTextBox.Text),
                    IsSinglePlayer = (bool)GameIsSinglePlayer.IsChecked,
                    StudioID = (int)TypeStudioComboBox.SelectedValue
                };
                db.Games.Add(newGame);
                db.SaveChanges();

                // Обновляем данные в dataGrid
                dataGrid.ItemsSource = db.Games.ToList();
            }


        }

        private void DeleteGameButton_Click(object sender, RoutedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                int gameId = int.Parse(GameIdTextBox.Text);
                var sel_game = db.Games.Where(x => x.ID == gameId).SingleOrDefault();
                if (sel_game != null)
                {
                    if (MessageBox.Show("Точно удалить?", "Подтверждение", MessageBoxButton.OKCancel,
                        MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        db.Games.Remove(sel_game);
                        db.SaveChanges();
                    }
                    // Обновляем данные в DataGrid
                    dataGrid.ItemsSource = db.Games.ToList();
                }
            }
        }

        private void StudioDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                // Получить выбранную строку
                Studios selectedStudio = (Studios)StudiosDataGrid.SelectedItem;

                // Если строка выбрана
                if (selectedStudio != null)
                {
                    // Заполнить элементы соответственно
                    StudioNameTextBox.Text = selectedStudio.Name;
                    StudioIdTextBox.Text = selectedStudio.ID.ToString();

                    var sel_StudioCountry = db.StudioCountry.SingleOrDefault(x => x.StudioID == selectedStudio.ID);
                    CountryComboBox.SelectedValue = sel_StudioCountry.CountryID;
                }
            }

        }

        private void FindStudioIdButton_Click(object sender, RoutedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                var sel_studio = db.Studios.Find(int.Parse(StudioIdTextBox.Text));

                if (sel_studio != null)
                {
                    // студия найдена, можно работать с ней
                    StudioNameTextBox.Text = sel_studio.Name;
                    //CountryComboBox.SelectedValue = sel_studio.StudioCountry.CountryID;

                }
                else
                {
                    MessageBox.Show("Студия не найдена по id");


                }
            }
        }

        private void FindGameButton_Click(object sender, RoutedEventArgs e)
        {
            using (GamesEntities db = new GamesEntities())
            {
                var sel_game = db.Games.Find(int.Parse(StudioIdTextBox.Text));

                if (sel_game != null)
                {
                    // игра найдена, можно работать с ней
                    // Вывести информацию об игре
                    GameTitleTextBox.Text = sel_game.Title;
                    GameGenreTextBox.Text = sel_game.Genre;
                    GameSalesTextBox.Text = sel_game.Sales.ToString();
                    GameIsSinglePlayer.IsChecked = sel_game.IsSinglePlayer;
                    TypeStudioComboBox.SelectedValue = sel_game.ID;
                }
                else
                {
                    MessageBox.Show("Игра не найдена по id");
                }
            }
        }

        private void AddStudioButton_Click(object sender, RoutedEventArgs e)
        {
            // Создать новый объект студии
            Studios newStudio = new Studios { Name = StudioNameTextBox.Text };
            // Создать новый объект Студия - Страна
            StudioCountry newStudioCountry = new StudioCountry();

            // Добавить студию в базу данных
            using (GamesEntities db = new GamesEntities())
            {
                if (db.Studios.Any(s => s.Name == newStudio.Name))
                {
                    MessageBox.Show("Студия уже существует");
                    return;
                }

                db.Studios.Add(newStudio);
                db.SaveChanges();

                // Обновить список студий
                TypeStudioComboBox.ItemsSource = db.Studios.ToList();

                int id_insertstudio = newStudio.ID;

                // Добавление Студия - Страна 

                newStudioCountry.CountryID = (int)CountryComboBox.SelectedValue;
                newStudioCountry.StudioID = id_insertstudio;
                db.StudioCountry.Add(newStudioCountry);
                db.SaveChanges();

            }
        }
    }
}
