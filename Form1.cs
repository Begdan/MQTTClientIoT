using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MQTTSubscriber
{
    public partial class Form1 : Form
    {
        private MqttClient mqttClient;
        
        public Form1()
        {
            InitializeComponent();
        }

        //Application that consumes messages from two MQTT brokers and displays them in a text box.
        //The application subscribes to the topic "itis/data" and to the topic itis/alarm.
        //The application will display the messages received from the topic "itis/data" and "itis/alarm" in the text box.

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                mqttClient = new MqttClient("broker.hivemq.com");
                mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
                mqttClient.Subscribe(new string[] { "itis/data", "itis/alert" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                mqttClient.Connect("MQTTSubscriber");
            });
        }

        private void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            var result = new Dictionary<string, string>();
            string messageToDisplay = "";

            try
            {
                result = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            }
            catch
            {
                //Do nothing
            }
            
            var messageBuilder = new StringBuilder();

            if (result != null && result.ContainsKey("topic"))
                switch (result["topic"])
                {
                    case "itis/data":
                        messageBuilder.AppendLine("Authentication - " + result["date_time"] + " " + "UserID: " +
                            result["user_id"] + " "
                            + "Password: " + result["password"] + " " + "Is Authorized: " + result["is_authorized"]);
                        break;
                    case "itis/alert":
                        messageBuilder.AppendLine("Alert - " + result["date_time"] + " " + "Message: " +
                            result["message"]);
                        break;
                }

            if (messageBuilder.Length > 0)
            {
                messageToDisplay = messageBuilder.ToString();
                listBox1.Invoke((MethodInvoker) (() => listBox1.Items.Add(messageToDisplay)));
            }
        }
    }
}