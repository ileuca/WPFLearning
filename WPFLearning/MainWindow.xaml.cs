using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace WPFLearning
{

    public partial class MainWindow : Window
    {
        HubConnection connection;
        public MainWindow()
        {
            InitializeComponent();

            connection = new HubConnectionBuilder().WithUrl("https://localhost:7017/chatHub").WithAutomaticReconnect().Build();

            connection.Reconnecting += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Attempting to reconnect...";
                    messages.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };

            connection.Reconnected += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Reconnected ...";
                    messages.Items.Clear();
                    messages.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };

            connection.Closed += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Connection Closed";
                    messages.Items.Clear();
                    messages.Items.Add(newMessage);
                    openConnection.IsEnabled = true;
                    sendMessage.IsEnabled = false;
                });

                return Task.CompletedTask;
            };
        }

        private async void openConnection_Click(object sender, RoutedEventArgs e)
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}: {message}";
                    messages.Items.Add(newMessage);
                });
            });

            try
            {
                await connection.StartAsync();
                messages.Items.Add("Connection Started");
                openConnection.IsEnabled = false;
                sendMessage.IsEnabled = true;
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }

        private async void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                await connection.InvokeAsync("SendMessage", userInput.Text, messageInput.Text);
                messageInput.Clear();
            }
            catch(Exception ex) 
            {
                messages.Items.Add(ex.Message);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                sendMessage_Click(sender, e);
            }
        }

    }
}
