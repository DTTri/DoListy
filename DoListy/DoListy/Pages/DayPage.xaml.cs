using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using DoListy.Services;
using Appointment = DoListy.ViewModel.Appointment;
using DoListy.Weather;

namespace DoListy.Pages;
public partial class DayPage : ContentPage
{
    private readonly IAudioManager audioManager;
    //Color for task in frame A
    private Color transparentColor = Color.FromRgba(255, 255, 255, 0);
    private Color blackColor = Color.FromRgb(255, 255,255); // it is white

    //set the setting task's day is Now (for tempo)            
    private DateTime temp = DateTime.Now;

    public DayPage(IAudioManager audioManager)
    {

        InitializeComponent();
        AlwaysOnDisplay(DateTime.Now);
        SetIniDisplayDate();
        this.audioManager = audioManager;
    }

    private void SetIniDisplayDate()
    {
        DateTime today = DateTime.Today;
        int delta = DayOfWeek.Monday - today.DayOfWeek; // Calculate the offset to Monday

        if (delta > 0)
            delta -= 7; // Adjust if today is later in the week than Monday

        // Adjust the condition to handle Monday separately
        if (delta == 0)
        {
            mon.DisplayDate = today;
        }
        else
        {
            mon.DisplayDate = today.AddDays(delta);
        }

        tue.DisplayDate = mon.DisplayDate.AddDays(1);
        wed.DisplayDate = mon.DisplayDate.AddDays(2);
        thus.DisplayDate = mon.DisplayDate.AddDays(3);
        fri.DisplayDate = mon.DisplayDate.AddDays(4);
        sat.DisplayDate = mon.DisplayDate.AddDays(5);
        sun.DisplayDate = mon.DisplayDate.AddDays(6);
    }
    private void LeftimaBut_Clicked(object sender, EventArgs e)
    {
        mon.DisplayDate = mon.DisplayDate.AddDays(-7);
        tue.DisplayDate = tue.DisplayDate.AddDays(-7);
        wed.DisplayDate = wed.DisplayDate.AddDays(-7);
        thus.DisplayDate = thus.DisplayDate.AddDays(-7);
        fri.DisplayDate = fri.DisplayDate.AddDays(-7);
        sat.DisplayDate = sat.DisplayDate.AddDays(-7);
        sun.DisplayDate = sun.DisplayDate.AddDays(-7);
    }

    private void RightimaBut_Clicked(object sender, EventArgs e)
    {
        mon.DisplayDate = mon.DisplayDate.AddDays(7);
        tue.DisplayDate = tue.DisplayDate.AddDays(7);
        wed.DisplayDate = wed.DisplayDate.AddDays(7);
        thus.DisplayDate = thus.DisplayDate.AddDays(7);
        fri.DisplayDate = fri.DisplayDate.AddDays(7);
        sat.DisplayDate = sat.DisplayDate.AddDays(7);
        sun.DisplayDate = sun.DisplayDate.AddDays(7);
    }

    private async void CheckBox_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.IsChecked)
        {
            var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("tick.mp3"));
            player.Play();
        }
    }

    private async void buttonAddTask_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddAppointmentPage));
        var add = (AddAppointmentPage)Shell.Current.CurrentPage;
        add.entryStartTime.Text = temp.ToString();
        add.pickerDateTime1.SelectedDate = temp;
        
        add.Disappearing += OnAddAppointmentPageDisappearing;
    }
    private void OnAddAppointmentPageDisappearing(object sender, EventArgs e)
    {
        RefreshCurrentFrame();
        AlwaysOnDisplay(temp);
    }
    private async Task AnimateFrames(Appointment input)
    {
        
            frame_A.WidthRequest = 350;
            await frame_A.TranslateTo(-220, 0, 250, Easing.Linear);
            Grid.SetColumn(frame_A, 0);
            Grid.SetRow(frame_A, 1);


            // Scale back to original size (if there's relevant code for this)
            frame_B.FindByName<Label>("TaskTitle").Text = input.Name;
            frame_B.FindByName<Label>("StartTime").Text = input.EventStart.ToString();
            frame_B.FindByName<Label>("EndTime").Text = input.EventEnd.ToString();
            //frame_B.FindByName<Label>("State").Text = input.;
            frame_B.FindByName<Label>("Notes").Text = input.Note;

            frame_B.IsVisible = true;

            await frame_B.FadeTo(1, 500, Easing.SinInOut); // Fade in
        
    }

    //Reset position
    private async void RefreshCurrentFrame()
    {
        // Reverse the visibility change for frame_B
        await frame_B.FadeTo(0, 500, Easing.SinInOut); // Fade out
        frame_B.IsVisible = false;
        // Move frame_A back to its original position
        frame_A.WidthRequest = 700;
        await frame_A.TranslateTo(0, 0, 250, Easing.Linear);
        Grid.SetColumn(frame_A, 0);
        Grid.SetRow(frame_A, 1);

    }
    private Appointment xxx;


    private void AlwaysOnDisplay(DateTime currentDate)
    {
        TaskDailyStack.Clear();
        List<Appointment> appointmentsForDate = new ObservableCollection<Appointment>(App.appointmentRepo.GetAppointments())
            .Where(app => app.EventStart.Date == currentDate.Date)
            .ToList();

        foreach (Appointment app in appointmentsForDate)
        {

            // Create labels for displaying appointment information
            Grid grid = CreateGrid();

            Label nameLabel = CreateLabel(app.Name, blackColor);
            CheckBox ckbox = CreateCheckBox();
            Label dateLabel = CreateDateLabel(app.EventStart, app.EventEnd, blackColor);
            ckbox.CheckedChanged += CheckBox_CheckedChanged;

            grid.Children.Add(nameLabel);
            grid.Children.Add(dateLabel);
            grid.Children.Add(ckbox);
            Grid.SetRow(nameLabel, 0);
            Grid.SetColumn(nameLabel, 0);

            Grid.SetRow(dateLabel, 1);
            Grid.SetColumn(dateLabel, 0);

            Grid.SetRowSpan(ckbox, 2);
            Grid.SetRow(ckbox, 0);
            Grid.SetColumn(ckbox, 1);  
            Grid.SetRowSpan(ckbox, 2);
            

            // Create a StackLayout to hold labels
            StackLayout infoStack = new StackLayout();
            infoStack.Children.Add(grid);

            // Create a Frame to contain appointment information
            Frame appointmentFrame = new Frame
            {
                BackgroundColor = transparentColor,
                Content = infoStack
            };

            // Add TapGestureRecognizer to the appointmentFrame
            appointmentFrame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await AnimateFrames(app); // Use the local variable                   
                })
            });

            // Create a StackLayout to hold the frame
            StackLayout frameStack = new StackLayout();
            frameStack.Children.Add(appointmentFrame);


            // Add the combined StackLayout to TaskDailyStack
            TaskDailyStack.Children.Add(frameStack);


        }

        // Separate helper methods to improve readability and maintainability

        Grid CreateGrid()
        {
            Grid newGrid = new Grid();
            newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            newGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            newGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            return newGrid;
        }

        Label CreateLabel(string text, Color textColor)
        {
            return new Label { Text = text, TextColor = textColor };
        }

        CheckBox CreateCheckBox()
        {
            return new CheckBox { IsChecked = false };
        }

        Label CreateDateLabel(DateTime start, DateTime end, Color textColor)
        {
            string formattedDate = $"{start:hh/mm,dd/mm/yy}-{end:hh/mm,dd/mm/yy}";
            return new Label { Text = formattedDate, TextColor = textColor };
        }







    }

    private void Butmon_Clicked(object sender, EventArgs e)
    {
        temp = this.mon.DisplayDate;
        RefreshCurrentFrame();
        AlwaysOnDisplay(mon.DisplayDate);

    }

    private void Buttue_Clicked(object sender, EventArgs e)
    {
        temp = this.tue.DisplayDate;
        RefreshCurrentFrame();
        AlwaysOnDisplay(tue.DisplayDate);
    }

    private void Butwed_Clicked(object sender, EventArgs e)
    {
        temp = this.wed.DisplayDate;
        RefreshCurrentFrame();
        AlwaysOnDisplay(wed.DisplayDate);
    }

    private void Butthus_Clicked(object sender, EventArgs e)
    {
        temp = this.thus.DisplayDate;
        RefreshCurrentFrame();
        AlwaysOnDisplay(thus.DisplayDate);

    }

    private void Butfri_Clicked(object sender, EventArgs e)
    {
        temp = this.fri.DisplayDate;
        RefreshCurrentFrame();
        AlwaysOnDisplay(fri.DisplayDate);

    }

    private void Butsat_Clicked(object sender, EventArgs e)
    {
        temp = this.sat.DisplayDate;
        RefreshCurrentFrame();
        AlwaysOnDisplay(sat.DisplayDate);

    }

    private void Butsun_Clicked(object sender, EventArgs e)
    {
        temp = this.sun.DisplayDate;
        RefreshCurrentFrame();
        AlwaysOnDisplay(sun.DisplayDate);

    }


    //Weather
    private string s = "";
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var result = await ApiService.getWeather(10.823, 106.6296);


        switch (result.current.weather_code)
        {
            //0
            default:
                
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/fluency/96/smiling-sun.png";

                }
                else
                {
                    s = "https://img.icons8.com/fluency/96/bright-moon.png";
                }

                break;

            case 1:
               
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/color/96/sun--v1.png";

                }
                else
                {
                    s = "https://img.icons8.com/color/96/night.png";
                }
                break;
            case 2:
                
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/fluency/96/partly-cloudy-day.png";

                }
                else
                {
                    s = "https://img.icons8.com/fluency/96/partly-cloudy-night.png";
                }
                break;
            case 3:
                
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/color/96/partly-cloudy-day--v1.png";

                }
                else
                {
                    s = "https://img.icons8.com/fluency/96/partly-cloudy-night.png";
                }
                break;

            case 45:
                
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/fluency/96/fog-day.png";

                }
                else
                {
                    s = "https://img.icons8.com/fluency/96/fog-night.png";
                }
                break;
            case 48:
                
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/fluency/96/fog-day.png";

                }
                else
                {
                    s = "https://img.icons8.com/fluency/96/fog-night.png";
                }
                break;




            case 61:
               

                s = "https://img.icons8.com/fluency/96/light-rain.png";


                break;

            case 63:
                
                s = "https://img.icons8.com/fluency/96/moderate-rain.png";
                break;
            case 65:
                
                s = "https://img.icons8.com/fluency/96/intense-rain.png";
                break;





            case 80:
                

                s = "https://img.icons8.com/fluency/96/light-rain.png";


                break;

            case 81:
                
                s = "https://img.icons8.com/fluency/96/moderate-rain.png";
                break;

            case 82:
                
                s = "https://img.icons8.com/fluency/96/intense-rain.png";
                break;



            case 95:
                
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/fluency/96/chance-of-storm.png";
                }
                else
                {
                    s = "https://img.icons8.com/plasticine/100/stormy-night.png";
                }
                break;

            case 96:
                
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/fluency/96/chance-of-storm.png";
                }
                else
                {
                    s = "https://img.icons8.com/plasticine/100/stormy-night.png";
                }
                break;
            case 99:
                
                if (result.current.is_day == 1)
                {
                    s = "https://img.icons8.com/fluency/96/chance-of-storm.png";
                }
                else
                {
                    s = "https://img.icons8.com/plasticine/100/stormy-night.png";
                }
                break;

        }
        weatherImage.Source = s;
        
    }

    private async void weatherImage_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(WeatherPage));
        
    }
}