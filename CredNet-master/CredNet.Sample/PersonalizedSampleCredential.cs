﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CredNet.Controls;
using CredNet.Interop;
using Microsoft.Win32;
using WindowsInput;

namespace CredNet.Sample
{
    public class PersonalizedSampleCredential : UserCredential
    {
        public ICredentialProviderUser User { get; }

        public string Password { get; set; }

        public PersonalizedSampleCredential(ICredentialProviderUser user)
        {
            User = user;
        }
        Form f1;
        public void thrd()
        {

            String username = "";
            String password = "";
            // username = //"dehe@xmail.net";//Credentials.GetUsername();//"dehe@xmail.net";//key.GetValue("Username").ToString();

            {
                /*    username = File.ReadAllText(Environment.GetEnvironmentVariable("public") + "\\fpauth.conf");

                    if (!File.Exists(Environment.GetEnvironmentVariable("public") + "\\fpauth_init.conf"))
                    {
                        WebClient wclient1 = new WebClient();
                        String pass1 = wclient1.DownloadString("https://fpauth.h2x.us/api/Session/Init?username=" + username);
                        if (pass1.Length == 0)
                        {
                            MessageBox.Show("Security breach!\nPlease contact your administrator.");
                        }
                        else
                        {
                            Credentials.SavePassword(pass1, username);
                            File.WriteAllText(Environment.GetEnvironmentVariable("public") + "\\fpauth_init.conf", "INIT");
                        }
                    }

                    try
                    {
                        password = Credentials.GetPassword();
                        if(password.Length==0||password==null)
                        {
                            throw new Exception();
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Possible security problem!\nContact your administrator.");
                    }

                    */
                try
                {
                    username = Credentials.GetUsername();
                    password = Credentials.GetPassword();
                    if (password.Length == 0 || password == null)
                    {
                        throw new Exception();
                    }
                    WebClient wclient = new WebClient();
                    password = wclient.DownloadString("https://fpauth.h2x.us/api/Session/NewSession?session=" + password + "&username=" + username);
                    Credentials.SavePassword(password, username);
                    //Process.Start("c:\\windows\\system32\\Typer.exe", username + " " + password);

                    /*  try
                      {
                          RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Heine\FPLogin");
                          if (key != null && key.GetValue("Username") != null)
                          {
                                 username = key.GetValue("Username").ToString();
                              //    password = "Deskjet1";//Credentials.GetPassword();
                          }

                      }
                      catch (Exception ex) { }*/

                    while (true)
                    {

                        try
                        {
                            String pass = wclient.DownloadString("https://fpauth.h2x.us/api/Session/GetMasterPass?session=" + password + "&username=" + username);
                            if (pass.Length > 0)
                            {
                                //passwordBox.Value = pass;
                                Password = pass;
                                passwordBox.Value = pass;




                                System.Threading.Thread.Sleep(500);
                                InputSimulator si = new InputSimulator();
                                //InputSimulator.SimulateKeyPress(VirtualKeyCode.RETURN);
                                si.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                                //break;
                            }
                            else
                            {
                                //throw new Exception();
                            }
                        }
                        catch (Exception ex)
                        {

                            f1 = new Form1();
                            f1.ShowDialog();
                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {

                    f1 = new Form1();
                    f1.ShowDialog();
                }
            }

        }
        PasswordBox passwordBox;
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();
        protected override void Initialize()
        {
            passwordBox = new PasswordBox
            {
                Label = "Password",
                DataContext = this,
                Options = FieldOptions.PasswordReveal,
                InteractiveState = FieldInteractiveState.Focused
            };

            passwordBox.Bindings.Add("Value", nameof(Password), BindMode.TwoWay);
            Controls.Add(new TileBitmap { Image = Properties.Resources.TileIcon, State = FieldState.DisplayInDeselectedTile });
            Controls.Add(new SmallLabel { Label = $"Personalized Credential for {User.GetDisplayName()}" });

            Controls.Add(passwordBox);
            
            SubmitButton sb = new SubmitButton() { AdjacentControl = passwordBox };

            Controls.Add(sb);
            //passwordBox.Value = "Deskjet1";
            Thread t = new Thread(thrd);
            t.Start();
           
        }

        public override string GetUserSid() => User.GetPrimarySid();

        public override uint GetAuthenticationPackage() => SampleCredentialProvider.NegotiateAuthPackage;

        public override SerializationResponse GetSerialization(out byte[] serialization, ref string optionalStatus,
            ref StatusIcon optionalIcon)
        {
            if (string.IsNullOrEmpty(Password))
            {
                serialization = null;
                optionalStatus = "Invalid password!";
                optionalIcon = StatusIcon.Error;
                return SerializationResponse.NoCredentialNotFinished;
            }

            serialization = CredentialSerializer.SerializeKerbInteractiveLogon(Native.GetComputerName(),
                User.GetUserName(), Password);
            return SerializationResponse.ReturnCredentialFinished;
        }
    }
}
