using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SimSIoT.DomainObjects;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace SimSIoT
{
    public partial class Simulator : Form
    {
        // Lists to store users and devices in the simulation
        static List<User> list_User;
        static List<Device> list_SR;
        static List<Device> list_SP;
        static List<Device> list_Dev;
      
        //static List<ServiceRequester> potential_SR;
        //static List<ServiceProvider> potential_SP;
        //static List<ServiceRequester> trusthworthiness_SR;
        //static List<ServiceProvider> trusthworthiness_SP;

        public Series seriesSuccessTrustOfRS = new System.Windows.Forms.DataVisualization.Charting.Series
        {
            Name = "seriesSuccessTrustOfRS",
            Color = System.Drawing.Color.DarkBlue,
            IsVisibleInLegend = false,
            IsXValueIndexed = true,
            ChartType = SeriesChartType.Spline,
            BorderWidth = 2
        };
        // Series for plotting success trust of service requesters

        public Series seriesTrustValueOfRS = new System.Windows.Forms.DataVisualization.Charting.Series
        {
            Name = "seriesTrustValueOfRS",
            Color = System.Drawing.Color.DarkBlue,
            IsVisibleInLegend = false,
            IsXValueIndexed = true,
            ChartType = SeriesChartType.Spline,
            BorderWidth = 2
        };
        // Series for plotting trust value of service requesters

        public Series seriesSatisficationOfRS = new System.Windows.Forms.DataVisualization.Charting.Series
        {
            Name = "seriesSatisficationOfRS",
            Color = System.Drawing.Color.DarkBlue,
            IsVisibleInLegend = false,
            IsXValueIndexed = true,
            ChartType = SeriesChartType.Spline,
            BorderWidth = 2
        };
        // Series for plotting satisfaction of service requesters

        public System.Diagnostics.Stopwatch TimeParameter = new System.Diagnostics.Stopwatch { };
    // Stopwatch to measure simulation time

        //************************************************SIMIULATOR*******************************************************************//
        /// <summary>
        /// Simulation of Social IoT
        /// Start point
        /// 1- Create users and his/her devices (number of users=200, average of devices for each user=2, some devices are SP and some ones are SR). generally there is 600 SP and SR.
        /// Transcation will do in "K" round
        /// 2- Select trustworthiness service providers (SP) by service requesters (SR) as potential SPs
        /// 3- Send requests from SR to selected SP with most trust value
        /// 4- Select trustworthiness SR by SP
        /// 5- SP accepts the delegation requestes and performs it.
        /// 6- Learning by SP and SR after finishing the transactions 
        /// 7- Transfer Friend Lists && Comunity Interests
        /// 8- Propagation of trust value by SP
        /// 9- Move to another place
        /// 10- Next round
        /// </summary>
        public Simulator()
        {
            InitializeComponent();
            // Initialize the form and simulation variables
            int time = 0;
            Random r = new Random();
            // Random number generator for selecting devices
            int selectedSR_Id, selectedSP_Good_Id, selectedSP_Bad_Id;
            Device selected_Device_SR, selected_Device_GoodSP, selected_Device_BadSP;
            list_User = new List<User>();
            list_SR = new List<Device>();
            list_SP = new List<Device>();
            list_Dev = new List<Device>();
            // Dictionaries to store selected service providers/requesters and results
            Dictionary<Device, double> selected_SP = new Dictionary<Device, double>();
            Dictionary<Device, double> selected_SR = new Dictionary<Device, double>();
            Dictionary<int, double> result_1 = new Dictionary<int, double>();
            Dictionary<int, List<double>> result_2 = new Dictionary<int, List<double>>();
            Dictionary<int, double> result_3 = new Dictionary<int, double>();
            Dictionary<int, double> result_4 = new Dictionary<int, double>();
            Dictionary<int, double> result_5 = new Dictionary<int, double>();

            Dictionary<int, double> result_6 = new Dictionary<int, double>();
            Dictionary<int, double> result_7 = new Dictionary<int, double>();

            TimeParameter.Start();
            // Start the simulation timer
            this.chartSuccessTrustOfSR.Series.Add(seriesSuccessTrustOfRS);
            this.chartTrustValueOfSR.Series.Add(seriesTrustValueOfRS);
            this.chartSatisfication.Series.Add(seriesSatisficationOfRS);
            // Add chart series to the respective charts

            ////*****step 1: create devices include service requesters and service providers
            //*****Selected one Node of SR to show the results of that
            ////*****Id of SP starts from 1 to 300 and for SP starts from 300 to 600
            // Select random IDs for SR and SP devices
            selectedSR_Id = r.Next(300, 600);
            selectedSP_Good_Id = r.Next(1, 300);
            selectedSP_Bad_Id = r.Next(1, 300);
            while (selectedSP_Bad_Id == selectedSP_Good_Id)
            {
                selectedSP_Bad_Id = r.Next(1, 300);
            }

            //Create_SP_SR();
            Create_SP_SR_RealData(selectedSR_Id, selectedSP_Good_Id, selectedSP_Bad_Id);
            // Create devices and assign roles using real data

            selected_Device_SR = list_Dev.Single(p => p.Id.Equals(selectedSR_Id));
            selected_Device_GoodSP = list_Dev.Single(p => p.Id.Equals(selectedSP_Good_Id));
            selected_Device_BadSP = list_Dev.Single(p => p.Id.Equals(selectedSP_Bad_Id));
            // Get references to the selected SR and SP devices
           
            ////*****Transcation will do in "K" round
            for (int k = 1; k <= 15; k++)
            {
                foreach (Device SR in list_SR)
                {
                    ////*****step 2: select trustworthiness service providers (SP) by service requesters (SR)
                    //Repeat for each SR
                    SR_PreEvaluation(SR,selected_Device_GoodSP,selected_Device_BadSP);
                    selected_SP = SR_Decision(SR);
                    SR.Selected_SP=selected_SP;
                    list_Dev.Single(p => p.Id.Equals(SR.Id)).Selected_SP = selected_SP;
                    list_SR.Single(p => p.Id.Equals(SR.Id)).Selected_SP = selected_SP;
                    ////*****step 3: send requests from SR to selected SP
                    SR_Send_Transaction(SR, selected_SP.Single().Key);
                }

                foreach (Device SP in list_SP)
                {
                    ////*****step 4:select trustworthiness SR by SPs
                    //Repeat it for each SR
                    SP_PreEvaluation(SP);
                    ////*****step 5:SP accepts the delegation requestes and performs it.
                    //assumption: SP selects all of potential SR and select_SR is the SR with most trust value
                    selected_SR = SP_Decision(SP);
                    SP.Selected_SR = selected_SR;
                    list_Dev.Single(p => p.Id.Equals(SP.Id)).Selected_SR = selected_SR;
                    list_SP.Single(p => p.Id.Equals(SP.Id)).Selected_SR = selected_SR;

                }
                
                ////*****step 6:Learning by SP and SR
                foreach (Device SR in list_SR)
                {
                    SR_PostEvaluation(SR, SR.Selected_SP);
                }

                foreach (Device SP in list_SP)
                {
                    //Assumption: SP accepted all of requetses from all SRs and do transaction with them and has PostEvaluation for learning for all of them
                    SP_PostEvaluation(SP);
                    //if (SP.Selected_SR.Count() != 0)
                    //    SP_PostEvaluation(SP, SP.Selected_SR);
                }

                ////*****step 7:Transfer Friend Lists && Comunity Interests
                //foreach (Device SR in list_SR)
                //{
                //    list_SR.Single(p => p.Id.Equals(SR.Id)).SR_IsTaskDone = true;
                //    TransferFriendLists_OneSide(SR);
                //}

                foreach (Device SP in list_SP)
                {
                   // TransferFriendLists_With_SelectedSR(SP);
                    TransferFriendLists_With_All(SP);
                }

                ////*****step 8:Propagation of trust value by SP
                ////*****step 9:Move to another place
                //SR_Move(SR);

                ////*****Show the result on the diagram
                ////Trust of SR of selected SPs
                ////Trust/Time
                //selected_Device = list_Dev.Single(p => p.Id.Equals(selected_Id));
                //time = int.Parse(Math.Floor(TimeParameter.Elapsed.TotalSeconds).ToString());
                //if (selected_Device.Selected_SP.Count() != 0)
                //    ShowChartTrustOfRS(time, selected_Device.Selected_SP.Single().Value);
                //else
                //    ShowChartTrustOfRS(time, 0);

                ////Real QoS of SR of SPs on optimal Qos of all potential SP
                //////Success Rate/Round
                double Delta_ground_Trust=0;
                
                if (selected_Device_SR.Potential_SP.Count() != 0)
                    Delta_ground_Trust = (double)selected_Device_SR.Selected_SP.Single().Key.ground_Trust / selected_Device_SR.Potential_SP.Keys.Max(p => p.ground_Trust);
                    //ground_Trust = (double)selected_Device.Selected_SP.Single().Key.ground_Trust / selected_Device.Potential_SP.Keys.Average(p => p.ground_Trust);

                if (selected_Device_SR.Selected_SP.Count() != 0)
                {
                    ShowChartSuccessTrustOfRS(k, Delta_ground_Trust);
                    ShowChartTrustValueOfRS(k, selected_Device_SR.Selected_SP.Single().Value);
                    ShowChartSatisfication(k, selected_Device_SR.Satisfication);

                    List<double> list = new List<double>();
                    list.Add(selected_Device_SR.Selected_SP.Single().Value);
                    list.Add(selected_Device_SR.Selected_SP.Single().Key.ground_Trust);

                    result_1.Add(k, Delta_ground_Trust);
                    result_2.Add(k, list);
                    result_3.Add(k, selected_Device_SR.Satisfication);
                }
                else
                {
                    ShowChartSuccessTrustOfRS(k, 0);
                    ShowChartTrustValueOfRS(k, 0);
                    ShowChartSatisfication(k, 0);

                    List<double> list = new List<double>();
                    list.Add(0);
                    list.Add(0);

                    result_1.Add(k, 0);
                    result_2.Add(k, list);
                    result_3.Add(k, 0);
                }

                ////************************ #it is inserted to considering the good and bad nodes ************************
               List<Device> List_related_selected_SRs_Good = list_Dev.Where(p => p.Role.Equals(Device.Device_Role.SR) && p.Selected_SP.Any(q=>q.Key.Equals(selected_Device_GoodSP))).ToList();
                List<double> trustValuesofGoodSP = new List<double>();
                List<double> trustMAEValuesofGoodSP = new List<double>();
                if (List_related_selected_SRs_Good.Count() > 0)
                {
                    foreach (Device SR in List_related_selected_SRs_Good)
                    {
                        double trustModel = SR.Selected_SP.Where(p => p.Key.Equals(selected_Device_GoodSP)).Single().Value;
                        double trustGround = selected_Device_GoodSP.ground_Trust;

                        trustValuesofGoodSP.Add(trustModel);
                        //trustMAEValuesofGoodSP.Add(Math.Abs(trustModel - trustGround));
                    }
                }
                else
                {
                    List<Device> List_related_Potential_SRs_Good = list_Dev.Where(p => p.Role.Equals(Device.Device_Role.SR) && p.Potential_SP.Any(q => q.Key.Equals(selected_Device_GoodSP))).ToList();
                    foreach (Device SR in List_related_Potential_SRs_Good)
                    {
                        double trustModel = SR.Potential_SP.Where(p => p.Key.Equals(selected_Device_GoodSP)).Single().Value;
                        double trustGround = selected_Device_GoodSP.ground_Trust;

                        trustValuesofGoodSP.Add(trustModel);
                        //trustMAEValuesofGoodSP.Add(Math.Abs(trustModel - trustGround));
                    }

              }
               
                //double AVG_trustValue_GoodSP = trustValuesofGoodSP.Average();
                //double MAEGoodNodes = trustMAEValuesofGoodSP.Average();
                double maxTrustGood = trustValuesofGoodSP.Max();
                double MAEGoodNodes = (double) (Math.Abs(trustValuesofGoodSP.Max() - selected_Device_GoodSP.ground_Trust) / 2);

                //result_4.Add(k, AVG_trustValue_GoodSP);
                result_4.Add(k, maxTrustGood);
                result_6.Add(k, MAEGoodNodes);

                List<Device> List_relatedSRs_Bad = list_Dev.Where(p => p.Role.Equals(Device.Device_Role.SR) && p.Potential_SP.Any(q => q.Key.Equals(selected_Device_BadSP))).ToList();
                List<double> trustValuesofBadSP = new List<double>();
                List<double> trustMAEValuesofBadSP = new List<double>();
                foreach (Device SR in List_relatedSRs_Bad)
                {
                    double trustModel = SR.Potential_SP.Where(p => p.Key.Equals(selected_Device_BadSP)).Single().Value;
                    double trustGround = selected_Device_BadSP.ground_Trust;

                    trustValuesofBadSP.Add(trustModel);
                    //trustMAEValuesofBadSP.Add(Math.Abs(trustModel - trustGround));
                }
                double AVG_trustValue_BadSP = trustValuesofBadSP.Max();
                double MAEBadNodes = (double) (Math.Abs(trustValuesofBadSP.Max() - selected_Device_BadSP.ground_Trust) / 2);
                //double maxTrustBad = trustValuesofBadSP.Max();


                result_5.Add(k, AVG_trustValue_BadSP);
                //result_5.Add(k, maxTrustBad);
                result_7.Add(k, MAEBadNodes);

                ////************************ #End ************************************************************************


                SaveIdofSelectedDevices(selectedSR_Id, selectedSP_Good_Id, selectedSP_Bad_Id);
                ////*****Next round
                //Setup_NewRound();
                Setup_NewRound(selectedSR_Id, selectedSP_Good_Id, selectedSP_Bad_Id);

            }

            SaveResultOfSimu(result_1, result_2, result_3, result_4, result_5, result_6, result_7);
        }

        /// <summary>
        /// Read node information from facebook_combined.txt
        /// Number of selected nodes is equal with 200
        /// The links of nodes is checked to be sure that are less than 200 b.c we will create just nodeId from 1 to 200 
        /// </summary>
        /// <returns>List of created node with their Id and their Links</returns>
        public Dictionary<int, List<int>> ReadNodes_SP_SR()
        {
            string[] lines_Links = System.IO.File.ReadAllLines("..\\..\\..\\..\\facebook_combined.txt");

            Dictionary<int, List<int>> Nodes = new Dictionary<int, List<int>>();
            List<int> list_Links = new List<int>();
            int id = 0, link = 0;
            ////*****Users
            foreach (string line in lines_Links)
            {
                string[] properties = line.Split(' ');

                for (int i = 0; i < properties.Count(); i++)
                {
                    switch (i)
                    {
                        case 0:
                            id = int.Parse(properties[0]);
                            break;
                        case 1:
                            link = int.Parse(properties[1]);
                            break;
                    }
                }
                if (link <= 200)
                {
                    if (!Nodes.Keys.Contains(id))
                    {
                        list_Links = new List<int>();
                        list_Links.Add(link);
                        Nodes.Add(id, list_Links);
                    }
                    else
                    {
                        Nodes.Where(p => p.Key.Equals(id)).Single().Value.Add(link);
                    }
                }
            }

            return Nodes;
        }

        /// <summary>
        /// Reads community information from a dataset file and constructs a dictionary mapping community IDs to lists of node IDs.
        /// </summary>
        /// <returns>Dictionary of community IDs and their member node IDs.</returns>
        public Dictionary<int, List<int>> ReadCommunity_SP_SR()
        {
            string[] lines_Community = System.IO.File.ReadAllLines("..\\..\\..\\..\\circles");

            Dictionary<int, List<int>> list_Community = new Dictionary<int, List<int>>();
            List<int> list_id = new List<int>();
            int community = 0;
            string[] nodes={};
            ////*****Users
             foreach (string line in lines_Community)
             {
                 string[] properties = line.Split(',');

                 for (int i = 0; i < properties.Count(); i++)
                 {
                     switch (i)
                     {
                         case 0:
                             community = int.Parse(properties[0]);
                             break;
                         case 1:
                             nodes = properties[1].Split('\t');
                             break;
                     }
                 }
                 list_id = new List<int>();
                 for (int i = 0; i < nodes.Count(); i++)
                 {
                     list_id.Add(int.Parse(nodes[i]));
                 }
                 list_Community.Add(community, list_id);
             }
             return list_Community;
        }

        /// <summary>
        /// Create users and devices just by random numbers
        /// </summary>
        /// <param name="selected_Id"></param>
        public void Create_SP_SR_Random(int selected_Id)
        {
            int j = 1;
            Random r = new Random();
            StreamWriter sw_User = new StreamWriter("..\\..\\..\\..\\User.txt");
            StreamWriter sw_SP = new StreamWriter("..\\..\\..\\..\\SP.txt");
            StreamWriter sw_SR = new StreamWriter("..\\..\\..\\..\\SR.txt");
            StreamWriter sw_Id = new StreamWriter("..\\..\\..\\..\\Id.txt");
            int service_num = 1;
            int subService_num = 1;
            int SR_bad_num = 1, SP_bad_num = 1;

            ////*****Read Real Data
            ReadNodes_SP_SR();
            ReadCommunity_SP_SR();

            ////*****Create Users
            for (int i = 1; i <= 20; i++)
            {
                User user = User.GetUser(i, r.Next(1, 5));

                for (int k = 1; k < 5; k++)
                {
                    int link_id = r.Next(1, 200);
                    while (user.Links.Contains(link_id))
                        link_id = r.Next(1, 200);
                    user.Links.Add(link_id);
                }
                list_User.Add(user);
            }
            ////*****Devices as Service Providers
            for (int i = 1; i < 100; i++)
            {
                Device dev = new Device();
                dev.Id = i;
                dev.Role = Device.Device_Role.SP;
                if (service_num == 5)
                    service_num = 1;
                else
                    service_num++;

                dev.Energy = r.Next(1, 4);
                dev.Computation = r.Next(1, 4);
                dev.Current_Location = DomainObjects.Unit.GetUnitByNumber(r.Next(1, 10));
                dev.Visited_Locations = new List<Unit.Type>();
                dev.Visited_Locations.Add(dev.Current_Location);
                //dev.Potential_SP = new List<Dictionary<Device, double>>();
                //dev.Trusthworthiness_SP = new List<Dictionary<Device, double>>();
                dev.Potential_SR = new Dictionary<Device, double>();
                dev.Trusthworthiness_SR = new Dictionary<Device, double>();
                dev.Visited_SR_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                dev.Visited_SP_Feedback = new Dictionary<Device, List<Context_Feedback>>();

                ////*****With node bads
                //// 10% bad nodes // 20% good nodes // 30% good nodes // 40% good nodes // 50% good nodes
                //if (SP_bad_num != 20)
                //{
                //    dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), 3, 3, 1, 1);
                //    dev.ground_Trust = Device.NextDouble(0.1, 0.2);
                //    SP_bad_num++;
                //}
                //else
                //{
                //    //dev.Services = Service.GetServiceForSP(r.Next(1, 5), r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                //    dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                //    dev.ground_Trust = Device.NextDouble(0.80, 0.95);
                //}

                //*****Without node bads
                dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                dev.ground_Trust = Device.NextDouble(0.80, 0.95);

                int user_ID = r.Next(1, 21);
                while (list_User.Where(p => p.Id.Equals(user_ID)).Single().Devices.Count() >= 6)
                    user_ID = r.Next(1, 21);
                dev.User = list_User.Where(p => p.Id.Equals(user_ID)).Single();
                list_User.Where(p => p.Id.Equals(user_ID)).Single().Devices.Add(dev.Id);

                //Writing in the text file 
                try
                {
                    //Write a line of text
                    sw_SP.WriteLine("Id_" + dev.Id + ",Role_1" + ",ServiceNum_" + Service.GetNumberByService(dev.Services.Services_Provided) + ",ServiceTime_" + Service.GetNumberByTime(dev.Services.Time_Service)
                        + ",ServiceLocation_" + Unit.GetNumberByUnit(dev.Services.Location_Service) + ",ServiceTimeResponce_" + Service.GetNumberByTimeResponse(dev.Services.Time_Response) + ",ServiceOoS_" + Service.GetNumberByQoS(dev.Services.OoS)
                        + ",ServiceTimeUsing_" + Service.GetNumberByTimeUsing(dev.Services.Time_Using) + ",ResourceUsing_" + Service.GetNumberByReosurcesUsing(dev.Services.Reosurces_Using)
                        + ",Energy_" + dev.Energy + ",Computation_" + dev.Computation + ",CurrentLocation_" + Unit.GetNumberByUnit(dev.Current_Location) + ",groundTrust_" + dev.ground_Trust
                        + ",UserId_" + dev.User.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }

                list_SP.Add(dev);
                list_Dev.Add(dev);
            }
            sw_SP.Close();

            ////*****Devices as Service Requesters
            for (int i = 100; i < 200; i++)
            {
                Device dev = new Device();
                dev.Id = i;
                dev.Role = Device.Device_Role.SR;
                if (service_num == 5)
                {
                    service_num = 1;
                    subService_num = 2;
                }
                else
                {
                    service_num++;
                    if (subService_num == 5)
                        subService_num = 1;
                    else
                        subService_num++;
                }

                dev.Energy = r.Next(1, 4);
                dev.Computation = r.Next(1, 4);
                dev.Current_Location = DomainObjects.Unit.GetUnitByNumber(r.Next(1, 10));
                dev.Visited_Locations = new List<Unit.Type>();
                dev.Visited_Locations.Add(dev.Current_Location);
                //dev.Potential_SP = new List<Dictionary<Device, double>>();
                //dev.Trusthworthiness_SP = new List<Dictionary<Device, double>>();
                dev.Potential_SP = new Dictionary<Device, double>();
                dev.Trusthworthiness_SP = new Dictionary<Device, double>();
                dev.Visited_SP_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                dev.Visited_SR_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                dev.ground_Trust = Device.NextDouble(0.85, 0.95);

                ////*****With node bads
                // 10% bad nodes // 20% good nodes // 30% good nodes // 40% good nodes // 50% good nodes
                //if (SR_bad_num != 20)
                //{
                //    //dev.Services = Service.GetServiceForSR(r.Next(1, 5), r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                //    dev.Services = Service.GetServiceForSR(service_num, r.Next(1, 4), r.Next(1, 10), 1, 1, 3, 3, r.Next(1, 3), r.Next(1, 3));
                //    dev.ground_Trust = Device.NextDouble(0.1, 0.2);
                //    SR_bad_num++;
                //}
                //else
                //{
                //    dev.Services = Service.GetServiceForSR(service_num, r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                //    dev.ground_Trust = Device.NextDouble(0.85, 0.95);

                //}

                //*****Without node bads
                dev.Services = Service.GetServiceForSR(service_num, subService_num, r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                dev.ground_Trust = Device.NextDouble(0.85, 0.95);

                int user_ID = r.Next(1, 21);
                while (list_User.Where(p => p.Id.Equals(user_ID)).Single().Devices.Count() >= 12)
                    user_ID = r.Next(1, 20);
                dev.User = list_User.Where(p => p.Id.Equals(user_ID)).Single();
                list_User.Where(p => p.Id.Equals(user_ID)).Single().Devices.Add(dev.Id);

                //Writing in the text file 
                try
                {
                    //Write a line of text
                    sw_SR.WriteLine("Id_" + dev.Id + ",Role_2" + ",ServiceNum_" + Service.GetNumberByService(dev.Services.Services_Requetsed) + ",SubServiceNum_" + Service.GetNumberByService(dev.Services.SubServices_Requetsed) + ",ServiceTime_" + Service.GetNumberByTime(dev.Services.Time_Service)
                        + ",ServiceLocation_" + Unit.GetNumberByUnit(dev.Services.Location_Service) + ",ServiceTimeResponce_" + Service.GetNumberByTimeResponse(dev.Services.Time_Response) + ",ServiceOoS_" + Service.GetNumberByQoS(dev.Services.OoS)
                        + ",ServiceTimeUsing_" + Service.GetNumberByTimeUsing(dev.Services.Time_Using) + ",ResourceUsing_" + Service.GetNumberByReosurcesUsing(dev.Services.Reosurces_Using)
                        + ",Energy_" + dev.Energy + ",Computation_" + dev.Computation + ",CurrentLocation_" + Unit.GetNumberByUnit(dev.Current_Location) + ",groundTrust_" + dev.ground_Trust
                        + ",UserId_" + dev.User.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }

                list_SR.Add(dev);
                list_Dev.Add(dev);
            }
            sw_SR.Close();

            //
            foreach (User user in list_User)
            {
                try
                {
                    //Write a line of text
                    sw_User.WriteLine("UserId_" + user.Id + ",UserProf_" + User.GetNumberByProf(user.Profession) + ",Links_" + string.Join("/", user.Links) + ",Devices_" + string.Join("/", user.Devices));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_User.Close();

            //Writing in the text file 
            try
            {
                //Write a line of text
                sw_Id.WriteLine("SelectedId_" + selected_Id+",");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            sw_Id.Close();
        }

        /// <summary>
        /// Read real data from dataset  
        /// Create users then the information (Id and links of nodes) which are read from the dataset is used.
        /// Assumtion is that each node is a user.
        /// Create devices in two groups as service provider and service requester
        /// Assumption is that each user has 2 devices as average
        /// Number of users=200; Number of devices totally=600 (SP=300 and SR=300)
        /// Assumption is that some nodes are good and some ones are bad
        /// Bad service providers provide bad services. Bad service requesters use in bad way of services. 
        /// </summary>
        /// <param name="selectedSR_Id">the Id of selected node to show the result for this specific node</param>
        public void Create_SP_SR_RealData(int selectedSR_Id, int  selectedSP_Good_Id, int selectedSP_Bad_Id)
        {
            Random r = new Random();
            //User.txt is for users information
            //SP.txt is for users information
            //SR.txt is for users information
            //Id.txt is for selected node (it shows a device of a user) to show the trust values for this specific node
            StreamWriter sw_User = new StreamWriter("..\\..\\..\\..\\User.txt");
            StreamWriter sw_SP = new StreamWriter("..\\..\\..\\..\\SP.txt");
            StreamWriter sw_SR = new StreamWriter("..\\..\\..\\..\\SR.txt");
            StreamWriter sw_Id = new StreamWriter("..\\..\\..\\..\\Id.txt");
            int service_num = 1;
            int subService_num = 1;

            int SR_bad_num = 1, SP_bad_num = 1;

            Dictionary<int, List<int>> list_Community = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> Nodes = new Dictionary<int, List<int>>();

            ////*****Read Real Data
            Nodes=ReadNodes_SP_SR();
            list_Community=ReadCommunity_SP_SR();

            ////*****Create users and match the readed nodes to created nodes as users
            for (int k = 0; k <= 200; k++)
            {
                User user = User.GetUser(k, r.Next(1, 5));
                if (Nodes.Any(p => p.Key.Equals(k)))
                {
                    List<int> list_Links = Nodes.Where(p => p.Key.Equals(k)).Single().Value;

                    for (int i = 0; i < list_Links.Count; i++)
                    {
                        user.Links.Add(list_Links[i]);
                    }

                    List<int> communites=new List<int>();
                    List<int> groupCommunites=new List<int>();

                    ////***** add the list of community for each user and each user has in a groupCommunities which is used to chech the Task Context in the Social Similarity
                    //// It means that users have to register in related communites with their requested tasks then it is possible to check the Task Context in Social Similarity
                    foreach (var communy in list_Community)
                    {
                        if (communy.Value.Contains(k))
                        {
                            communites.Add(communy.Key);
                            switch (communy.Key)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    groupCommunites.Add(1);
                                    break;
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                    groupCommunites.Add(2);
                                    break;
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                    groupCommunites.Add(3);
                                    break;
                                case 15:
                                case 16:
                                case 17:
                                case 18:
                                case 19:
                                    groupCommunites.Add(4);
                                    break;
                                case 20:
                                case 21:
                                case 22:
                                case 23:
                                    groupCommunites.Add(5);
                                    break;
                            }
                        }
                    }
                    user.Communities = communites;
                    user.GroupCommunities = groupCommunites;
                }
                list_User.Add(user);
            }
            ////*****Create Devices as Service Providers
            for (int i = 1; i < 300; i++)
            {
                Device dev = new Device();
                dev.Id = i;
                dev.Role = Device.Device_Role.SP;
                if (service_num == 5)
                {
                    service_num = 1;
                    subService_num = 2;
                }
                else
                {
                    service_num++;
                    if (subService_num == 5)
                        subService_num = 1;
                    else
                        subService_num++;
                }

                dev.Energy = r.Next(1, 4);
                dev.Computation = r.Next(1, 4);
                dev.Current_Location = DomainObjects.Unit.GetUnitByNumber(r.Next(1, 10));
                dev.Visited_Locations = new List<Unit.Type>();
                dev.Visited_Locations.Add(dev.Current_Location);
                //dev.Potential_SP = new List<Dictionary<Device, double>>();
                //dev.Trusthworthiness_SP = new List<Dictionary<Device, double>>();
                dev.Potential_SR = new Dictionary<Device, double>();
                dev.Trusthworthiness_SR = new Dictionary<Device, double>();
                dev.Visited_SR_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                dev.Visited_SP_Feedback = new Dictionary<Device, List<Context_Feedback>>();

                ////************************ #it is inserted to considering the good and bad nodes ************************
                ////*****With node bads
                //// 10% bad nodes // 20% bad nodes 20% good nodes= (120 total and 60 each)// 30% bad nodes (180 total and 90 each) // 40% bad nodes  (240 total and 120 each) // 50% bad nodes (300 total and 150 each)
                //if (dev.Id.Equals(selectedSP_Bad_Id))
                //{
                //    dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), 3, 3, 1, 1);
                //    dev.ground_Trust = Device.NextDouble(0.55, 0.60);
                //    SP_bad_num++;
                //}
                //else if (dev.Id.Equals(selectedSP_Good_Id))
                //{
                //    dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), 1, 1, 1, 1);
                //    dev.ground_Trust = 0.85;
                //}
                //else if (SP_bad_num != 150)
                //{
                //    dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), 3, 3, 1, 1);
                //    dev.ground_Trust = Device.NextDouble(0.55, 0.60);
                //    SP_bad_num++;
                //}
                //else
                //{
                //    //dev.Services = Service.GetServiceForSP(r.Next(1, 5), r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                //    dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                //    dev.ground_Trust = Device.NextDouble(0.80, 0.85);
                //}
                ////************************ #End *************************************************************************

                ////*************Without node bads****************************************
                dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                dev.ground_Trust = Device.NextDouble(0.80, 0.85);
                ////*************End*******************************************************

                int user_ID = r.Next(0, 200);
                while (list_User.Where(p => p.Id.Equals(user_ID)).Single().Devices.Count() >= 2)
                    user_ID = r.Next(0, 200);
                dev.User = list_User.Where(p => p.Id.Equals(user_ID)).Single();
                list_User.Where(p => p.Id.Equals(user_ID)).Single().Devices.Add(dev.Id);


                //Writing in the text file 
                try
                {
                    //Write a line of text
                    sw_SP.WriteLine("Id_" + dev.Id + ",Role_1" + ",ServiceNum_" + Service.GetNumberByService(dev.Services.Services_Provided) + ",ServiceTime_" + Service.GetNumberByTime(dev.Services.Time_Service)
                        + ",ServiceLocation_" + Unit.GetNumberByUnit(dev.Services.Location_Service) + ",ServiceTimeResponce_" + Service.GetNumberByTimeResponse(dev.Services.Time_Response) + ",ServiceOoS_" + Service.GetNumberByQoS(dev.Services.OoS)
                        + ",ServiceTimeUsing_" + Service.GetNumberByTimeUsing(dev.Services.Time_Using) + ",ResourceUsing_" + Service.GetNumberByReosurcesUsing(dev.Services.Reosurces_Using)
                        + ",Energy_" + dev.Energy + ",Computation_" + dev.Computation + ",CurrentLocation_" + Unit.GetNumberByUnit(dev.Current_Location) + ",groundTrust_" + dev.ground_Trust
                        + ",UserId_" + dev.User.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }

                list_SP.Add(dev);
                list_Dev.Add(dev);
            }
            sw_SP.Close();

            ////*****Create Devices as Service Requesters
            for (int i = 300; i < 600; i++)
            {
                Device dev = new Device();
                dev.Id = i;
                dev.Role = Device.Device_Role.SR;
                if (service_num == 5)
                    service_num = 1;
                else
                    service_num++;

                dev.Energy = r.Next(1, 4);
                dev.Computation = r.Next(1, 4);
                dev.Current_Location = DomainObjects.Unit.GetUnitByNumber(r.Next(1, 10));
                dev.Visited_Locations = new List<Unit.Type>();
                dev.Visited_Locations.Add(dev.Current_Location);
                //dev.Potential_SP = new List<Dictionary<Device, double>>();
                //dev.Trusthworthiness_SP = new List<Dictionary<Device, double>>();
                dev.Potential_SP = new Dictionary<Device, double>();
                dev.Trusthworthiness_SP = new Dictionary<Device, double>();
                dev.Visited_SP_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                dev.Visited_SR_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                //dev.ground_Trust = Device.NextDouble(0.85, 0.85);

                ////************************ #it is inserted to considering the good and bad nodes ************************
                ////*****With node bads
                //// 10% bad nodes // 20% bad nodes 20% good nodes= (120 total and 60 each)// 30% bad nodes (180 total and 90 each) // 40% bad nodes  (240 total and 120 each) // 50% bad nodes (300 total and 150 each)
                if (SR_bad_num != 150)
                {
                    //dev.Services = Service.GetServiceForSR(r.Next(1, 5), r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                    dev.Services = Service.GetServiceForSR(service_num, subService_num, r.Next(1, 4), r.Next(1, 10), 1, 1, 3, 3, r.Next(1, 3), r.Next(1, 3));
                    dev.ground_Trust = Device.NextDouble(0.55, 0.60);
                    SR_bad_num++;
                }
                else
                {
                    dev.Services = Service.GetServiceForSR(service_num, subService_num, r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                    dev.ground_Trust = Device.NextDouble(0.80, 0.85);
                }
                ////************************ #End *************************************************************************


                ////*********************** Without node bads**************************************************************
                //dev.Services = Service.GetServiceForSR(service_num, subService_num, r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                //dev.ground_Trust = Device.NextDouble(0.80, 0.85);
                //************************* #End **************************************************************************

                int user_ID = r.Next(0, 200);
                while (list_User.Where(p => p.Id.Equals(user_ID)).Single().Devices.Count() >= 5)
                    user_ID = r.Next(0, 200);
                dev.User = list_User.Where(p => p.Id.Equals(user_ID)).Single();
                list_User.Where(p => p.Id.Equals(user_ID)).Single().Devices.Add(dev.Id);
                
                //Writing in the text file 
                try
                {
                    //Write a line of text
                    sw_SR.WriteLine("Id_" + dev.Id + ",Role_2" + ",ServiceNum_" + Service.GetNumberByService(dev.Services.Services_Requetsed) + ",SubServiceNum_" + Service.GetNumberByService(dev.Services.SubServices_Requetsed) + ",ServiceTime_" + Service.GetNumberByTime(dev.Services.Time_Service)
                        + ",ServiceLocation_" + Unit.GetNumberByUnit(dev.Services.Location_Service) + ",ServiceTimeResponce_" + Service.GetNumberByTimeResponse(dev.Services.Time_Response) + ",ServiceOoS_" + Service.GetNumberByQoS(dev.Services.OoS)
                        + ",ServiceTimeUsing_" + Service.GetNumberByTimeUsing(dev.Services.Time_Using) + ",ResourceUsing_" + Service.GetNumberByReosurcesUsing(dev.Services.Reosurces_Using)
                        + ",Energy_" + dev.Energy + ",Computation_" + dev.Computation + ",CurrentLocation_" + Unit.GetNumberByUnit(dev.Current_Location) + ",groundTrust_" + dev.ground_Trust
                        + ",UserId_" + dev.User.Id + ",ServiceCost_" + dev.Services.Cost + ",ServiceSpeed_" + dev.Services.Speed);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }

                list_SR.Add(dev);
                list_Dev.Add(dev);
            }
            sw_SR.Close();

            //
            foreach (User user in list_User)
            {
                try
                {
                    //Write a line of text
                    sw_User.WriteLine("UserId_" + user.Id + ",UserProf_" + User.GetNumberByProf(user.Profession) + ",Links_" + string.Join("/", user.Links) + ",Communities_" + string.Join("/", user.Communities)
                        + ",GroupCommunities_" + string.Join("/", user.GroupCommunities) + ",Devices_" + string.Join("/", user.Devices));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_User.Close();

            //Writing in the text file 
            try
            {
                //Write a line of text
                sw_Id.WriteLine("SelectedSRId_" + selectedSR_Id.ToString()+ ",SelectedSPGoodId_" + selectedSP_Good_Id.ToString()+ ",SelectedSPBadId_" + selectedSP_Bad_Id.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            sw_Id.Close();
        }

        //************************************************GENERAL FUNCTIONS*************************************************************//

        /// <summary>
        /// Calculates the location distance between a service requester and provider.
        /// </summary>
        /// <returns>Distance as an integer (1 if same location, otherwise inverse of absolute difference).</returns>
        public int Get_Location_Distance(Device SR, Device SP)
        {
            int distance=1;

            if (SR.Services.Location_Service.Equals(SP.Services.Location_Service))
                distance = 1;
            else
                distance = 1/(Math.Abs(Unit.GetNumberByUnit(SR.Services.Location_Service) - Unit.GetNumberByUnit(SP.Services.Location_Service)));

            return distance;
        }

        /// <summary>
        /// Calculates the time distance between a service requester and provider.
        /// </summary>
        /// <returns>Distance as an integer (1 if same time, otherwise inverse of absolute difference).</returns>
        public int Get_Time_Distance(Device SR, Device SP)
        {
            int distance = 1;

            if (SR.Services.Time_Service.Equals(SP.Services.Time_Service))
                distance = 1;
            else
                distance = 1 / (Math.Abs(Service.GetNumberByTime(SR.Services.Time_Service) - Service.GetNumberByTime(SP.Services.Time_Service)));
            return distance;
        }

        /// <summary>
        /// Calculates the profession distance between a service requester and provider.
        /// </summary>
        /// <returns>Distance as an integer (1 if same profession, otherwise 0.5).</returns>
        public int Get_Profession_Distance(Device SR, Device SP)
        {
            int distance = 1;

            if (SR.User.Profession.Equals(SP.User.Profession))
                distance = 1;
            else
                distance = 1 / 2; //?????????????
            return distance;
        }

        /// <summary>
        /// Placeholder for decay calculation based on feedbacks (currently returns 1).
        /// </summary>
        public double Get_Decay(List<double> feedbacks)
        {
            double decay = 1;
            return decay;
        }

        /// <summary>
        /// Calculates the coefficient of variation for a list of feedback values.
        /// </summary>
        /// <returns>Coefficient of variation (standard deviation divided by mean).</returns>
        public double Get_Variation(List<double> feedbacks)
        {
            //double Coefficient_variation = 1;
            double Coefficient_variation = 0;
            double Mean = 0; // absolute value
            double Standard = 0; // standard deviation
            double sum = 0, sum_3 = 0;
            //double skewness = 0,  Mean_3=0;

            if (feedbacks.Count > 1)
            {
                Mean = feedbacks.Sum() / feedbacks.Count();
                //feedbacks.Sort();
                for (int i = 0; i < feedbacks.Count(); i++)
                {
                    sum += Math.Pow(feedbacks[i] - Mean, 2);
                    //sum_3 += Math.Pow(feedbacks[i] - Mean, 3);
                }


                Standard = sum / (feedbacks.Count() - 1);

                //Mean_3 = sum / feedbacks.Count();

                Coefficient_variation = Math.Sqrt(Standard) / Mean;
            }

            //skewness = Mean_3 / Math.Pow(Math.Sqrt(Standard), 3);
            //skewness = 3*(Mean - feedbacks[feedbacks.Count/2]) / Math.Sqrt(Standard);

            return Coefficient_variation;
        }

        /// <summary>
        /// This function is for next transactions in next rounds
        /// </summary>
        public void Setup_NewRound()
        {
            Random r = new Random();
            List<int> count = new List<int>();
            int number = 0;
            int service_num = 1;
            int subService_num = 1;

            //////***** In real word, some service requesters change to service providers and visa versa
            for (int i = 0; i < 100; i++)
            {
                ////***** Selecting a random number from the id ranges of SR and SP which should not be duplicate
                number = r.Next(1, 200);
                while (count.Contains(number))
                {
                    number = r.Next(1, 200);
                }
                count.Add(number);

                //////***** Finding the divice with the selected id to change the its role 
                Device dev = list_Dev.Single(p => p.Id.Equals(number));
                if (dev.Role == Device.Device_Role.SP)
                {
                    dev.Role = Device.Device_Role.SR;
                    //dev.Services = Service.GetServiceForSR(r.Next(1, 5), r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                    
                    if (service_num == 5)
                    {
                        service_num = 1;
                        subService_num = 2;
                    }
                    else
                    {
                        service_num++;
                        if (subService_num == 5)
                            subService_num = 1;
                        else
                            subService_num++;
                    }

                    dev.Services = Service.GetServiceForSR(service_num, subService_num, r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                    if (list_SP.Exists(p => p.Id.Equals(dev.Id)))
                        list_SP.Remove(dev);
                    if (!list_SR.Exists(p => p.Id.Equals(dev.Id)))
                        list_SR.Add(dev);
                }
                else if (dev.Role == Device.Device_Role.SR)
                {
                    dev.Role = Device.Device_Role.SP;
                    //dev.Services = Service.GetServiceForSP(r.Next(1, 5), r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                    if (service_num == 5)
                        service_num = 1;
                    else
                        service_num++;
                    dev.Services = Service.GetServiceForSP(service_num, r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                    dev.SR_IsTaskDone = false;
                    if (list_SR.Exists(p => p.Id.Equals(dev.Id)))
                        list_SR.Remove(dev);
                    if (!list_SP.Exists(p => p.Id.Equals(dev.Id)))
                        list_SP.Add(dev);
                }
            }

            for (int i = 0; i < 600; i++)
            {

                //////***** Removing the prevous information from SR
                list_Dev.Single(p => p.Id.Equals(i)).Potential_SP = new Dictionary<Device, double>();
                list_Dev.Single(p => p.Id.Equals(i)).Trusthworthiness_SP = new Dictionary<Device, double>();
                //list_Dev.Single(p => p.Id.Equals(i)).Visited_SP_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                list_Dev.Single(p => p.Id.Equals(i)).Selected_SP = new Dictionary<Device, double>();

                //////***** Removing the prevous information from SP
                list_Dev.Single(p => p.Id.Equals(i)).Potential_SR = new Dictionary<Device, double>();
                list_Dev.Single(p => p.Id.Equals(i)).Trusthworthiness_SR = new Dictionary<Device, double>();
                //list_Dev.Single(p => p.Id.Equals(i)).Visited_SR_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                list_Dev.Single(p => p.Id.Equals(i)).Selected_SR = new Dictionary<Device, double>();
            }
        }

        /// <summary>
        /// Prepares the simulation for a new round, ensuring selected devices retain their roles, and resets round-specific data.
        /// </summary>
        public void Setup_NewRound(double selectedSR_Device, double selectedSPGood_Device, double selectedSPBad_Device)
        {
            Random r = new Random();
            List<int> count = new List<int>();
            int number = 0;
            int service_num_SP = 1;
            int service_num_SR = 1;
            int subService_num = 1;

            //////***** In real word, some service requesters change to service providers and visa versa
            for (int i = 1; i < 200; i++)
            {
                ////***** Selecting a random number from the id ranges of SR and SP which should not be duplicate
                number = r.Next(1, 600);
                //selected_Device is a service requester which is selected in previous steps to show the trust values of potential service providers selected by this service requester.
                while (count.Contains(number) || number == selectedSR_Device || number == selectedSPGood_Device || number == selectedSPBad_Device)
                {
                    number = r.Next(1, 600);
                }
                count.Add(number);

                //////***** Finding the divice with the selected id to change the its role 
                Device dev = list_Dev.Single(p => p.Id.Equals(number));
                if (dev.Role == Device.Device_Role.SP)
                {
                    dev.Role = Device.Device_Role.SR;
                    //dev.Services = Service.GetServiceForSR(r.Next(1, 5), r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                    if (service_num_SR == 5)
                    {
                        service_num_SR = 1;
                        subService_num = 2;
                    }
                    else
                    {
                        service_num_SR++;
                        if (subService_num == 5)
                            subService_num = 1;
                        else
                            subService_num++;
                    }
                    dev.Services = Service.GetServiceForSR(service_num_SR, subService_num, r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                    if (list_SP.Exists(p => p.Id.Equals(dev.Id)))
                        list_SP.Remove(dev);
                    if (!list_SR.Exists(p => p.Id.Equals(dev.Id)))
                        list_SR.Add(dev);
                }
                else if (dev.Role == Device.Device_Role.SR)
                {
                    dev.Role = Device.Device_Role.SP;
                    //dev.Services = Service.GetServiceForSP(r.Next(1, 5), r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                    if (service_num_SP == 5)
                        service_num_SP = 1;
                    else
                        service_num_SP++;
                    dev.Services = Service.GetServiceForSP(service_num_SP, r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                    dev.SR_IsTaskDone = false;
                    if (list_SR.Exists(p => p.Id.Equals(dev.Id)))
                        list_SR.Remove(dev);
                    if (!list_SP.Exists(p => p.Id.Equals(dev.Id)))
                        list_SP.Add(dev);
                }
            }

            service_num_SP = 1;
            service_num_SR = 1;

            ////***** Update the information of other service providers and service requesters
            for (int i = 1; i < 600; i++)
            {
                //////***** Removing the prevous information from SR
                list_Dev.Single(p => p.Id.Equals(i)).Potential_SP = new Dictionary<Device, double>();
                list_Dev.Single(p => p.Id.Equals(i)).Trusthworthiness_SP = new Dictionary<Device, double>();
                //list_Dev.Single(p => p.Id.Equals(i)).Visited_SP_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                list_Dev.Single(p => p.Id.Equals(i)).Selected_SP = new Dictionary<Device, double>();

                //////***** Removing the prevous information from SP
                list_Dev.Single(p => p.Id.Equals(i)).Potential_SR = new Dictionary<Device, double>();
                list_Dev.Single(p => p.Id.Equals(i)).Trusthworthiness_SR = new Dictionary<Device, double>();
                //list_Dev.Single(p => p.Id.Equals(i)).Visited_SR_Feedback = new Dictionary<Device, List<Context_Feedback>>();
                list_Dev.Single(p => p.Id.Equals(i)).Selected_SR = new Dictionary<Device, double>();

                ////***** change the previous information of SPs and SRs while they are not selected to change their role b.c those selected ones has updated before.
                if (!count.Contains(i) || number != selectedSR_Device || number != selectedSPGood_Device || number != selectedSPBad_Device)
                {
                    Service service = new Service();
                    Device dev = list_Dev.Single(p => p.Id.Equals(i));
                    if (dev.Role == Device.Device_Role.SP)
                    {
                        if (service_num_SP == 5)
                            service_num_SP = 1;
                        else
                            service_num_SP++;

                        service = Service.GetServiceForSP(service_num_SP, r.Next(1, 4), r.Next(1, 10), r.Next(1, 3), r.Next(1, 3), 1, 1);
                        list_Dev.Single(p => p.Id.Equals(i)).Services = service;
                        list_SP.Single(p => p.Id.Equals(i)).Services = service;

                    }
                    else if (dev.Role == Device.Device_Role.SR)
                    {
                        if (service_num_SR == 5)
                        {
                            service_num_SR = 1;
                            subService_num = 2;
                        }
                        else
                        {
                            service_num_SR++;
                            if (subService_num == 5)
                                subService_num = 1;
                            else
                                subService_num++;
                        }

                        service = Service.GetServiceForSR(service_num_SR, subService_num, r.Next(1, 4), r.Next(1, 10), 1, 1, r.Next(1, 3), r.Next(1, 3), r.Next(1, 3), r.Next(1, 3));
                        list_Dev.Single(p => p.Id.Equals(i)).Services = service;
                        list_SR.Single(p => p.Id.Equals(i)).Services = service;
                    }
                }
            }
        }

        /// <summary>
        /// This function is designed for showing the results on the charts with different parameters
        /// </summary>
        /// <param name="round"></param>
        /// <param name="trust"></param>
        public void ShowChartSuccessTrustOfRS(int round, double trust)
        {
            /////Show Chart
            if (chartSuccessTrustOfSR.InvokeRequired)
                chartSuccessTrustOfSR.BeginInvoke((MethodInvoker)delegate
                {
                    seriesSuccessTrustOfRS.Points.AddXY(round, trust);
                    seriesSuccessTrustOfRS.ChartType = SeriesChartType.Line;
                    chartSuccessTrustOfSR.Invalidate();
                });
            else
            {
                seriesSuccessTrustOfRS.Points.AddXY(round, trust);
                seriesSuccessTrustOfRS.ChartType = SeriesChartType.Line;
                chartSuccessTrustOfSR.Invalidate();
            }
        }

        /// <summary>
        /// Plots the trust value of service requesters on the chart for a given round.
        /// </summary>
        public void ShowChartTrustValueOfRS(int round, double trust)
        {
            /////Show Chart
            if (chartTrustValueOfSR.InvokeRequired)
                chartTrustValueOfSR.BeginInvoke((MethodInvoker)delegate
                {
                    seriesTrustValueOfRS.Points.AddXY(round, trust);
                    seriesTrustValueOfRS.ChartType = SeriesChartType.Line;
                    chartTrustValueOfSR.Invalidate();
                });
            else
            {
                seriesTrustValueOfRS.Points.AddXY(round, trust);
                seriesTrustValueOfRS.ChartType = SeriesChartType.Line;
                chartTrustValueOfSR.Invalidate();
            }
        }

        /// <summary>
        /// Plots the satisfaction of service requesters on the chart for a given round.
        /// </summary>
        public void ShowChartSatisfication(int round, double Satisfication)
        {
            /////Show Chart
            if (chartSatisfication.InvokeRequired)
                chartSatisfication.BeginInvoke((MethodInvoker)delegate
                {
                    seriesSatisficationOfRS.Points.AddXY(round, Satisfication);
                    seriesSatisficationOfRS.ChartType = SeriesChartType.Line;
                    chartSatisfication.Invalidate();
                });
            else
            {
                seriesSatisficationOfRS.Points.AddXY(round, Satisfication);
                seriesSatisficationOfRS.ChartType = SeriesChartType.Line;
                chartSatisfication.Invalidate();
            }
        }

        /// <summary>
        /// Saves simulation results to text files for later analysis.
        /// </summary>
        public void SaveResultOfSimu(Dictionary<int, double> result_SuccessRate, Dictionary<int, List<double>> result_TrustValue, Dictionary<int, double> result_Satisfication, Dictionary<int, double> result_TrustGoodNod, Dictionary<int, double> result_TrustBadNod, Dictionary<int, double> result_MAEGoodNodes, Dictionary<int, double> result_MAEBadNodes)
        {
            ////Save in the file
            StreamWriter sw_ResultSuccessRate = new StreamWriter("..\\..\\..\\..\\Result_1_SimSIoT_Success.txt");
            foreach (var item in result_SuccessRate)
            {
                try
                {
                    //Write a line of text
                    //sw_Result1.WriteLine(item.Key.ToString() + "," + item.Value);
                    sw_ResultSuccessRate.WriteLine(item.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_ResultSuccessRate.Close();

            ////Save in the file
            StreamWriter sw_ResultTrustValue = new StreamWriter("..\\..\\..\\..\\Result_1_SimSIoT_TrustValue.txt");
            foreach (var item in result_TrustValue)
            {
                try
                {
                    //Write a line of text
                    //sw_Result1.WriteLine(item.Key.ToString() + "," + item.Value);
                    sw_ResultTrustValue.WriteLine(item.Value[0].ToString() + ", " + item.Value[1].ToString());
                    //sw_ResultTrustValue.WriteLine(item.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_ResultTrustValue.Close();

            ////Save in the file
            StreamWriter sw_ResultSatisfication = new StreamWriter("..\\..\\..\\..\\Result_1_SimSIoT_Satisfication.txt");
            foreach (var item in result_Satisfication)
            {
                try
                {
                    //Write a line of text
                    //sw_Result1.WriteLine(item.Key.ToString() + "," + item.Value);
                    sw_ResultSatisfication.WriteLine(item.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_ResultSatisfication.Close();

            ////Save in the file
            StreamWriter sw_ResultTrustGoodNode = new StreamWriter("..\\..\\..\\..\\Result_1_SimSIoT_TrustGoodNode.txt");
            foreach (var item in result_TrustGoodNod)
            {
                try
                {
                    //Write a line of text
                    //sw_Result1.WriteLine(item.Key.ToString() + "," + item.Value);
                    sw_ResultTrustGoodNode.WriteLine(item.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_ResultTrustGoodNode.Close();

            ////Save in the file
            StreamWriter sw_ResultTrustBadNode = new StreamWriter("..\\..\\..\\..\\Result_1_SimSIoT_TrustBadNode.txt");
            foreach (var item in result_TrustBadNod)
            {
                try
                {
                    //Write a line of text
                    //sw_Result1.WriteLine(item.Key.ToString() + "," + item.Value);
                    sw_ResultTrustBadNode.WriteLine(item.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_ResultTrustBadNode.Close();

            ////Save in the file
            StreamWriter sw_ResultTrustMAEGooNode = new StreamWriter("..\\..\\..\\..\\Result_1_SimSIoT_MAEGoodNode.txt");
            foreach (var item in result_MAEGoodNodes)
            {
                try
                {
                    //Write a line of text
                    //sw_Result1.WriteLine(item.Key.ToString() + "," + item.Value);
                    sw_ResultTrustMAEGooNode.WriteLine(item.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_ResultTrustMAEGooNode.Close();

            ////Save in the file
            StreamWriter sw_ResultTrustMAEBadNode = new StreamWriter("..\\..\\..\\..\\Result_1_SimSIoT_MAEBadNode.txt");
            foreach (var item in result_MAEBadNodes)
            {
                try
                {
                    //Write a line of text
                    //sw_Result1.WriteLine(item.Key.ToString() + "," + item.Value);
                    sw_ResultTrustMAEBadNode.WriteLine(item.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }
            }
            sw_ResultTrustMAEBadNode.Close();
        }

        /// <summary>
        /// Saves the IDs of selected devices to a text file for tracking.
        /// </summary>
        public void SaveIdofSelectedDevices(double selectedSR_Id, double selectedSPGoodId,double selectedSPBadId)
        {
            //Writing in the text file 
            StreamWriter sw_Id = new StreamWriter("..\\..\\..\\..\\AllIds.txt");
            try
            {
                //Write a line of text
                sw_Id.WriteLine("SelectedSRId_" + selectedSR_Id + ",selectedSPGoodId_" + selectedSPGoodId + ",selectedSPBadId" + selectedSPBadId);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            sw_Id.Close();
        }

        /// <summary>
        /// Transfers friend lists between a service provider and its selected service requester.
        /// </summary>
        public void TransferFriendLists_With_SelectedSR(Device SP)
        {
            List<int> list_New_SP = new List<int>();
            List<int> list_New_SR = new List<int>();

            if (SP.Selected_SR.Count() != 0)
            {
                ////***** Sharing Freinds 
                list_New_SP = new List<int>();
                list_New_SR = new List<int>();

                for (int i = 0; i < SP.User.Links.Count(); i++)
                {
                    if (!SP.Selected_SR.Single().Key.User.Links.Contains(SP.User.Links[i]) && !list_New_SR.Contains(SP.User.Links[i]))
                    {
                        list_New_SR.Add(SP.User.Links[i]);
                        break;
                    }
                }

                for (int i = 0; i < SP.Selected_SR.Single().Key.User.Links.Count(); i++)
                {
                    if (!SP.User.Links.Contains(SP.Selected_SR.Single().Key.User.Links[i]) && !list_New_SR.Contains(SP.Selected_SR.Single().Key.User.Links[i]))
                    {
                        list_New_SP.Add(SP.Selected_SR.Single().Key.User.Links[i]);
                        break;
                    }
                }
                foreach (int item in SP.User.Links)
                {
                    if (!list_New_SP.Contains(item))
                        list_New_SP.Add(item);
                }

                list_SP.Single(p => p.Id.Equals(SP.Id)).User.Links = list_New_SP;
                list_Dev.Single(p => p.Id.Equals(SP.Id)).User.Links = list_New_SP;

                foreach (int item in SP.Selected_SR.Single().Key.User.Links)
                {
                    if (!list_New_SR.Contains(item))
                        list_New_SR.Add(item);
                }

                list_SR.Single(p => p.Id.Equals(SP.Selected_SR.Single().Key.Id)).User.Links = list_New_SR;
                list_Dev.Single(p => p.Id.Equals(SP.Selected_SR.Single().Key.Id)).User.Links = list_New_SR;
            }

        }

        /// <summary>
        /// Transfers friend lists between a service provider and all its trustworthy service requesters.
        /// </summary>
        public void TransferFriendLists_With_All(Device SP)
        {
            List<int> list_New_SP = new List<int>();
            List<int> list_New_SR = new List<int>();

            foreach(var SR in SP.Trusthworthiness_SR)
            {
                ////***** Sharing Freinds 
                list_New_SP = new List<int>();
                list_New_SR = new List<int>();

                for (int i = 0; i < SP.User.Links.Count(); i++)
                {
                    if (!SR.Key.User.Links.Contains(SP.User.Links[i]) && !list_New_SR.Contains(SP.User.Links[i]))
                    {
                        list_New_SR.Add(SP.User.Links[i]);
                        break;
                    }
                }

                for (int i = 0; i < SR.Key.User.Links.Count(); i++)
                {
                    if (!SP.User.Links.Contains(SR.Key.User.Links[i]) && !list_New_SR.Contains(SR.Key.User.Links[i]))
                    {
                        list_New_SP.Add(SR.Key.User.Links[i]);
                        break;
                    }
                }

                foreach (int item in SP.User.Links)
                {
                    if (!list_New_SP.Contains(item))
                        list_New_SP.Add(item);
                }

                list_SP.Single(p => p.Id.Equals(SP.Id)).User.Links = list_New_SP;
                list_Dev.Single(p => p.Id.Equals(SP.Id)).User.Links = list_New_SP;

                foreach (int item in SR.Key.User.Links)
                {
                    if (!list_New_SR.Contains(item))
                        list_New_SR.Add(item);
                }

                list_SR.Single(p => p.Id.Equals(SR.Key.Id)).User.Links = list_New_SR;
                list_Dev.Single(p => p.Id.Equals(SR.Key.Id)).User.Links = list_New_SR;
            }

        }

        /// <summary>
        /// Transfers friend lists from a service requester to its selected service provider (one-sided).
        /// </summary>
        public void TransferFriendLists_OneSide(Device SR)
        {
            List<int> list_New_SP = new List<int>();
            List<int> list_New_SR = new List<int>();

            if (SR.Selected_SP.Count() != 0)
            {
                ////***** Sharing Freinds 
                list_New_SP = new List<int>();
                list_New_SR = new List<int>();

                for (int i = 0; i < SR.User.Links.Count(); i++)
                {
                    if (!SR.Selected_SP.Single().Key.User.Links.Contains(SR.User.Links[i]) && !list_New_SR.Contains(SR.User.Links[i]))
                    {
                        list_New_SP.Add(SR.User.Links[i]);
                        break;
                    }
                }

                for (int i = 0; i < SR.Selected_SP.Single().Key.User.Links.Count(); i++)
                {
                    if (!SR.User.Links.Contains(SR.Selected_SP.Single().Key.User.Links[i]) && !list_New_SR.Contains(SR.Selected_SP.Single().Key.User.Links[i]))
                    {
                        list_New_SR.Add(SR.Selected_SP.Single().Key.User.Links[i]);
                        break;
                    }
                }

                foreach (int item in SR.User.Links)
                {
                    if (!list_New_SR.Contains(item))
                        list_New_SR.Add(item);
                }

                list_SR.Single(p => p.Id.Equals(SR.Id)).User.Links = list_New_SR;
                list_Dev.Single(p => p.Id.Equals(SR.Id)).User.Links = list_New_SR;

                foreach (int item in SR.Selected_SP.Single().Key.User.Links)
                {
                    if (!list_New_SP.Contains(item))
                        list_New_SP.Add(item);
                }

                list_SP.Single(p => p.Id.Equals(SR.Selected_SP.Single().Key.Id)).User.Links = list_New_SP;
                list_Dev.Single(p => p.Id.Equals(SR.Selected_SP.Single().Key.Id)).User.Links = list_New_SP;

            }

        }

        //************************************************SERVICE REQUESTER*************************************************************//
        /// <summary>
        /// Pre_Evalution from SR to SP
        /// Then, Get Decision by SR to arrange the most trustworthiness SP by their trust value
        /// </summary>
        /// <returns></returns>
        public void SR_PreEvaluation(Device SR, Device selected_Device_GoodSP, Device selected_Device_BadSP)
        {
            ////***** 1. select service providers who provide the requested service 
            List<Device> p = new List<Device>();
            p.AddRange(list_SP.Where(sp => sp.Services.Services_Provided.Equals(SR.Services.Services_Requetsed) || sp.Services.Services_Provided.Equals(SR.Services.SubServices_Requetsed)).ToList<Device>());
            //p.AddRange(list_SP.Where(sp => sp.Services.Services_Provided.Equals(SR.Services.Services_Requetsed)));

            ////************************ #it is inserted to considering the good and bad nodes ************************
            if (!p.Any(q => q.Id.Equals(selected_Device_GoodSP.Id)))
            {
                selected_Device_GoodSP.Services.Services_Provided = SR.Services.Services_Requetsed;
                selected_Device_GoodSP.Services.Time_Service = SR.Services.Time_Service;
                selected_Device_GoodSP.Services.Location_Service = SR.Services.Location_Service;
                //selected_Device_GoodSP.Energy = SR.Energy;
                //selected_Device_GoodSP.Computation = SR.Computation;
                //selected_Device_GoodSP.User.Profession = SR.User.Profession;

                p.Add(selected_Device_GoodSP);
            }
            if (!p.Any(q => q.Id.Equals(selected_Device_BadSP.Id)))
            {
                //selected_Device_BadSP.Services.Services_Provided = SR.Services.Services_Requetsed;
                
                //*************
                //selected_Device_BadSP.Services.Services_Provided = SR.Services.Services_Requetsed;
                //selected_Device_BadSP.Services.Time_Service = SR.Services.Time_Service;
                //selected_Device_BadSP.Services.Location_Service = SR.Services.Location_Service;
                //*************

                //selected_Device_BadSP.Energy = SR.Energy;
                //selected_Device_BadSP.Computation = SR.Computation;
                //selected_Device_BadSP.User.Profession = SR.User.Profession;
                p.Add(selected_Device_BadSP);
            }
            ////************************ End **************************************************************************

            ////***** 2. computing trust_value for each SP from SR to have potential service providers
            foreach (Device SP in p)
            {
                double trust_SR_SP = 0;
                //Dictionary<Device, double> dictionary_Trust = new Dictionary<Device, double>();
                trust_SR_SP = SR_TrustComputing_SP(SR, SP);
                //dictionary_Trust.Add(SP, trust_SR_SP);
                //SR.Potential_SP.Add(dictionary_Trust);
                SR.Potential_SP.Add(SP, trust_SR_SP);
            }
            ////*****???????? if trust value of Potential_SP is more than XX add into Trusthworthiness_SP
            SR.Trusthworthiness_SP = SR.Potential_SP;
        }

        /// <summary>
        /// Computes the trust value from a service requester to a service provider.
        /// </summary>
        /// <returns>Trust value as a double.</returns>
        public double SR_TrustComputing_SP(Device SR, Device SP)
        {
            double trust_SR_SP=0;
            //double A = 0.7;
            //double B = 0.1;
            double A = 0.8;
            double tt = Math.Pow(Math.E, 0);
            //trust_SR_SP = A * SR_Contextual_Mutuality_Trust_SP(SR, SP) + (1-A) * SR_Contextual_Social_Trust_SP(SR, SP);
            trust_SR_SP = A * SR_Contextual_Mutuality_Trust_SP(SR, SP) + (1-A) * SR_Contextual_Social_Trust_SP(SR, SP);

            return trust_SR_SP;
        }

        //***************//
        /// <summary>
        /// Calculates the contextual mutuality trust between a service requester and provider based on context vectors and feedback.
        /// </summary>
        /// <returns>Mutuality trust value as a double.</returns>
        public double SR_Contextual_Mutuality_Trust_SP(Device SR, Device SP)
        {
            double trust_SR_CMT_SP = 0, trust_SR_CMT_QoS_SP = 0, trust_SR_CMT_Social_SP = 0;
            double decay = 1, variation = 1;
            double C = 0.5;
            double norm_Vector_SR = 0, norm_Vector_SP = 0, angle_Vectors = 0, dotproduct = 0;
            ////*****
            Context_Feedback context_SP = new Context_Feedback();
            Context_Feedback context_SR = new Context_Feedback();
            List<Context_Feedback> context_feedbacks = new List<Context_Feedback>();
            List<double> feedbacks = new List<double>();

            ////***** Calculation Trust of Standars and Trust of QoS
            //old---trust_SR_CMT_QoS_SP = SR.Energy * SP.Energy + SR.Computation * SP.Computation + Get_Location_Distance(SR, SP) + Get_Time_Distance(SP, SR) +Get_Profession_Distance(SR,SP);

            ////***** Contexts of SP
            context_SP.Energy = SP.Energy;
            context_SP.Computation = SP.Computation;
            context_SP.Profession = User.GetNumberByProf(SP.User.Profession);
            context_SP.Location_Service = Unit.GetNumberByUnit(SP.Services.Location_Service);
            context_SP.Time_Service = Service.GetNumberByTime(SP.Services.Time_Service);
            ////***Computing the workload by time and location
            Random r = new Random();
            //Context_Feedback.WorkLoad WL = Context_Feedback.GetWorkLoadByNumber(r.Next(1, 5));
            //int WL_int = Context_Feedback.GetNumberByWorkLoad(WL);
            //context_SP.Workloads = WL_int;
            context_SP.Workloads = r.Next(1,5);

            ////***** Context of SR
            context_SR.Energy = SR.Energy;
            context_SR.Computation = SR.Computation;
            context_SR.Profession = User.GetNumberByProf(SR.User.Profession);
            context_SR.Location_Service = Unit.GetNumberByUnit(SR.Services.Location_Service);
            context_SR.Time_Service = Service.GetNumberByTime(SR.Services.Time_Service);
            ////*** We assump that the best workload which is computed by time and locatin is equal to 1 for SR
            context_SR.Workloads = 1;

            ////***** Calculation angle between vectors of service provider and service requesters in the N-dimention Space of Contexst (Environment,Device)
            //norm_Vector_SP= Math.Sqrt(Math.Pow(context_SP.Energy,2) +Math.Pow(context_SP.Computation,2)+
            //   Math.Pow(context_SP.Profession, 2) + Math.Pow(context_SP.Location_Service, 2) + Math.Pow(context_SP.Time_Service, 2));

            //norm_Vector_SR = Math.Sqrt(Math.Pow(context_SR.Energy, 2) + Math.Pow(context_SR.Computation, 2) +
            //Math.Pow(context_SR.Profession, 2) + Math.Pow(context_SR.Location_Service, 2) + Math.Pow(context_SR.Time_Service, 2));

            //dotproduct = context_SP.Energy * context_SR.Energy +  context_SP.Computation * context_SR.Computation +
            //    context_SP.Profession * context_SR.Profession + context_SP.Location_Service * context_SR.Location_Service + context_SP.Time_Service * context_SR.Time_Service;

            norm_Vector_SP = Math.Sqrt(Math.Pow(context_SP.Energy, 2) + Math.Pow(context_SP.Computation, 2) +
              Math.Pow(context_SP.Profession, 2) + Math.Pow(context_SP.Workloads, 2));

            norm_Vector_SR = Math.Sqrt(Math.Pow(context_SR.Energy, 2) + Math.Pow(context_SR.Computation, 2) +
            Math.Pow(context_SR.Profession, 2) + Math.Pow(context_SR.Workloads, 2));

            dotproduct = context_SP.Energy * context_SR.Energy + context_SP.Computation * context_SR.Computation +
                context_SP.Profession * context_SR.Profession + context_SP.Workloads * context_SR.Workloads;

            
            ////angle_Vectors = Math.Acos(dotproduct / (norm_Vector_SP * norm_Vector_SR));
            angle_Vectors = dotproduct / (norm_Vector_SP * norm_Vector_SR);
            trust_SR_CMT_QoS_SP = angle_Vectors;

            trust_SR_CMT_Social_SP = SR_Trust_SimilaritySocial_SP(SR, SP);
            double distance = 1 - trust_SR_CMT_Social_SP;

            ////***** For first intiraction, trustor should use the feedback 
            //if (SR.Visited_SP_Feedback.Count() == 0 || (SR.Visited_SP_Feedback.Any(p => p.Key.Id.Equals(SP.Id)) && SR.Visited_SP_Feedback.Single(q => q.Key.Id.Equals(SP.Id)).Value.Count()==0))
            if (SR.Visited_SP_Feedback.Count() == 0)
            {
                //trust_SR_CMT_SP = trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance);
                trust_SR_CMT_SP = C * trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance);
            }
            else if (SR.Visited_SP_Feedback.Any(p => p.Key.Id.Equals(SP.Id)) && SR.Visited_SP_Feedback.Single(q => q.Key.Id.Equals(SP.Id)).Value.Count() == 0)
            {
                //trust_SR_CMT_SP = trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance);
                trust_SR_CMT_SP = C * trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance);
            }
            else if (SR.Visited_SP_Feedback.Any(p => p.Key.Id.Equals(SP.Id)) && SR.Visited_SP_Feedback.Single(q => q.Key.Id.Equals(SP.Id)).Value.Count() != 0)
            {
                context_feedbacks = SR.Visited_SP_Feedback.Single(p => p.Key.Id.Equals(SP.Id)).Value;
                feedbacks = context_feedbacks.Select(p => p.Feedback).ToList();
                decay = Get_Decay(feedbacks);
                variation = Get_Variation(feedbacks);

                //trust_SR_CMT_SP = trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance) +decay * variation * feedbacks[feedbacks.Count() - 1];
                //trust_SR_CMT_SP = trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance) + Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1];
                //trust_SR_CMT_SP = (double)(trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance) + Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1]) / (1 + feedbacks.Max());
                trust_SR_CMT_SP = C * trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance) + (1-C) * Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1];

            }
            return trust_SR_CMT_SP;
        }

        /// <summary>
        /// Calculates the social similarity trust between a service requester and provider based on friends and communities.
        /// </summary>
        /// <returns>Social similarity trust value as a double.</returns>
        public double SR_Trust_SimilaritySocial_SP(Device SR, Device SP)
        {
            double trust_SR_Similarity_SP = 0;
            double sum = 0;
            double sim_F = 0, sim_Com = 0, sim_Cen = 0;
            double w_F=0, w_Com=0, w_Cen=0;

            //Calculating w_F, w_L, w_C while w_F + w_L + w_C = 1
            //w_F = 0.33; w_Com = 0.33; w_Cen = 0.33;
            w_F = 1;

            ////***** Calculating Friend similarity :sim_F
            ////***** Which we consdier Task Context. if SR and SP have a friend in common, he or she shoud register in the same community group 
            ////***** that shows that freind has same group related to requested task context.
            List<int> list_friendship_Similarity = new List<int>();
            for (int i = 0; i < SR.User.Links.Count(); i++)
            {
                if (SP.User.Links.Contains(SR.User.Links[i]))
                {
                    if (list_User[SR.User.Links[i]].GroupCommunities == SR.User.GroupCommunities)
                        list_friendship_Similarity.Add(1);
                }
            }

            //if (list_friendship_Similarity.Count() != 0 && (SR.User.GroupCommunities.Count() * SP.User.GroupCommunities.Count()) != 0)
            if (list_friendship_Similarity.Count() != 0 && (SR.User.Links.Count() * SP.User.Links.Count()) != 0)
                sim_F = (double)list_friendship_Similarity.Count() / (Math.Sqrt(SR.User.Links.Count() * SP.User.Links.Count()));
            else
                sim_F = 0;

            ////***** Calculating Community of Interest Similarity : sim_Com
            
            List<int> list_GroupComunity_Similarity = new List<int>();
            for (int i = 0; i < SR.User.GroupCommunities.Count(); i++)
            {
                for (int j = 0; j < SP.User.GroupCommunities.Count(); j++)
                {
                    if (SR.User.GroupCommunities[i].Equals(SP.User.GroupCommunities[j]))
                        list_GroupComunity_Similarity.Add(1);
                }
            }
            if (list_GroupComunity_Similarity.Count() != 0 && (SR.User.GroupCommunities.Count() * SP.User.GroupCommunities.Count()) !=0)
                sim_Com = (double)list_GroupComunity_Similarity.Count() / (Math.Sqrt(SR.User.GroupCommunities.Count() * SP.User.GroupCommunities.Count()));
            else
                sim_Com = 0;

            
            //
            trust_SR_Similarity_SP = w_F * sim_F + w_Com * sim_Com + w_Cen * sim_Cen;
            return trust_SR_Similarity_SP;
        }
     
        //***************//Recommender
        /// <summary>
        /// Calculates the contextual social trust from a service requester to a provider using recommendations from friends.
        /// </summary>
        /// <returns>Contextual social trust value as a double.</returns>
        public double SR_Contextual_Social_Trust_SP(Device SR, Device SP)
        {
            double trust_SR_CST_SP = 0;
            double trust_SR_Recommends_Direct_SP = 0, trust_SR_Sum_Recommends_SP = 0;
            double similarity = 0, sum_sum_Similarity = 0, sum_num = 0, sum_Distance_Recomenders = 0, sum_Similarity_Contexts_Recommender = 0;
            List<double> sum_Similarity = new List<double>();
            double trust_direct_recommender = 0, SR_Similarity_Contexts_Recommender=0, SR_Similarity_Contexts_Recommender_=0;
            double distance=0, distance_=0;
            Context_Feedback Context_Feedback = new DomainObjects.Context_Feedback(); 
            Dictionary<Device, double> Dic_SimSP = new Dictionary<Device, double>();
            Dictionary<Device, double> Dic_SimSR = new Dictionary<Device, double>();
            Dictionary<int, double> list_Distance = new Dictionary<int, double>();
            Dictionary<int, double> list_Similarity = new Dictionary<int, double>();


            //????????????????? Recommenders are just friends??
            //calculating similarity with friends as Recommenders
            for (int i = 0; i < SR.User.Links.Count(); i++)
            {
                int id_Recommender = SR.User.Links[i];
                if (list_SP.Exists(p => p.Id.Equals(id_Recommender)))
                {
                    similarity = SR_Trust_Similarity_RecommenderSP(SR, list_SP.Single(p => p.Id.Equals(id_Recommender)));

                    Dic_SimSP.Add(list_SP.Single(q => q.Id.Equals(id_Recommender)), similarity);
                }
                else
                {
                    similarity = SR_Trust_Similarity_RecommenderSR(SR, list_SR.Single(p => p.Id.Equals(id_Recommender)));

                    Dic_SimSR.Add(list_SR.Single(q => q.Id.Equals(id_Recommender)), similarity);
                }

                ////*******************Select the highest similarti
                //list_SR_similarity.Add(Dic_Sim);
            }
            //step1: select friends with highest similarity

            //Step1-1: compute the sum social similarity trust
            for (int i = 0; i < SR.User.Links.Count(); i++)
            {
                int id_Recommender = SR.User.Links[i];
                if (list_SP.Exists(p => p.Id.Equals(id_Recommender)))
                {
                    distance = 1 - Dic_SimSP.Single(p => p.Key.Id.Equals(id_Recommender)).Value;
                    SR_Similarity_Contexts_Recommender = SR_Similarity_Contexts_Recommender_SP(SR, Context_Feedback);
                }
                else
                {
                    distance = 1 - Dic_SimSR.Single(p => p.Key.Id.Equals(id_Recommender)).Value;
                    SR_Similarity_Contexts_Recommender = SR_Similarity_Contexts_Recommender_SP(SR, Context_Feedback);

                }

                list_Distance.Add(id_Recommender, distance);
                list_Similarity.Add(id_Recommender, SR_Similarity_Contexts_Recommender);

                sum_Distance_Recomenders += Math.Pow(Math.E, -distance);
                sum_Similarity_Contexts_Recommender+=SR_Similarity_Contexts_Recommender;
            }
            //

            //step2: select trust value recomendation of selected friends
            for (int i = 0; i < SR.User.Links.Count(); i++)
            {
                int id_Recommender = SR.User.Links[i];
                if (list_SP.Exists(p => p.Id.Equals(id_Recommender)))
                {
                    //distance=1- Dic_SimSP.Single(p => p.Key.Id.Equals(id_Recommender)).Value;
                    distance=list_Distance.Single(p => p.Key.Equals(id_Recommender)).Value;
                    if (distance == 0)
                    {
                        distance_ = 0;
                    }
                    else
                    {
                        distance_ = (double)(Math.Pow(Math.E, -distance)) / (sum_Distance_Recomenders);
                    }

                    //SR_Similarity_Contexts_Recommender=SR_Similarity_Contexts_Recommender_SP(SR, Context_Feedback);
                    SR_Similarity_Contexts_Recommender = list_Similarity.Single(p => p.Key.Equals(id_Recommender)).Value;
                    if (SR_Similarity_Contexts_Recommender == 0)
                    {
                        SR_Similarity_Contexts_Recommender_ = 0;
                    }
                    else
                    {
                        SR_Similarity_Contexts_Recommender_ = (double)(SR_Similarity_Contexts_Recommender) / (sum_Similarity_Contexts_Recommender);
                    }

                    //(Trust value Service Recommender to Service Provider) = previous feedbacks of service recommender to service provider. if it dosen't have feed back, mutuality trust is calculated.
                    Context_Feedback = SR_Contextual_Mutuality_Trust__Recommender_SP(list_SP.Single(p => p.Id.Equals(id_Recommender)), SP);
                    //trust Service Requester to Service Recommender = (similarity Service Requester with Service Recommender as Weight) * (Trust value Service Recommender to Service Provider) * similarity context of Service Requster to context of Service Recommenders.
                    //trust_direct_recommender = Context_Feedback.Feedback * Math.Pow(Math.E, -distance) * SR_Similarity_Contexts_Recommender_SP(list_SP.Single(p => p.Id.Equals(id_Recommender)), Context_Feedback);
                    //trust_direct_recommender = Context_Feedback.Feedback * Math.Pow(Math.E, -distance) * SR_Similarity_Contexts_Recommender_SP(SR, Context_Feedback);
                    trust_direct_recommender = Context_Feedback.Feedback * distance_ *  SR_Similarity_Contexts_Recommender_;
                }
                else
                {
                    //distance=1- Dic_SimSR.Single(p => p.Key.Id.Equals(id_Recommender)).Value;
                    distance = list_Distance.Single(p => p.Key.Equals(id_Recommender)).Value;
                    if (distance == 0)
                    {
                        distance_ = 0;
                    }
                    else
                    {
                        distance_ = (double)(Math.Pow(Math.E, -distance)) / (sum_Distance_Recomenders);
                    }

                    //SR_Similarity_Contexts_Recommender= SR_Similarity_Contexts_Recommender_SP(SR, Context_Feedback);
                    SR_Similarity_Contexts_Recommender = list_Similarity.Single(p => p.Key.Equals(id_Recommender)).Value;
                    if (SR_Similarity_Contexts_Recommender == 0)
                    {
                        SR_Similarity_Contexts_Recommender_ = 0;
                    }
                    else
                    {
                        SR_Similarity_Contexts_Recommender_ = (double)(SR_Similarity_Contexts_Recommender) / (sum_Similarity_Contexts_Recommender);
                    }

                    // (Trust value Service Recommender to Service Provider) = previous feedbacks of service recommender to service provider. if it dosen't have feed back, mutuality trust is calculated.
                    Context_Feedback = SR_Contextual_Mutuality_Trust__Recommender_SP(list_SR.Single(p => p.Id.Equals(id_Recommender)), SP);
                    //trust Service Requester to Service Recommender = (similarity Service Requester with Service Recommender as Weight) * (Trust value Service Recommender to Service Provider) * similarity context of Service Requster to context of Service Recommenders.
                    //trust_direct_recommender = Context_Feedback.Feedback * Math.Pow(Math.E, -distance) * SR_Similarity_Contexts_Recommender_SP(list_SR.Single(p => p.Id.Equals(id_Recommender)), Context_Feedback);
                    //trust_direct_recommender = Context_Feedback.Feedback * Math.Pow(Math.E, -distance) * SR_Similarity_Contexts_Recommender_SP(SR, Context_Feedback); 
                    trust_direct_recommender = Context_Feedback.Feedback * distance_ * SR_Similarity_Contexts_Recommender_;
                }
                sum_num += 1;
                sum_sum_Similarity += trust_direct_recommender;
                //sum_Similarity.Add(trust_direct_recommender);

            }

            //trust_SR_Sum_Recommends_SP = (double)sum_sum_Similarity;
            trust_SR_Sum_Recommends_SP = sum_sum_Similarity;
            return trust_SR_Sum_Recommends_SP;
        }

        /// <summary>
        /// Calculates the similarity between a service requester and a recommender (who is also a service requester).
        /// </summary>
        /// <returns>Similarity value as a double.</returns>
        public double SR_Trust_Similarity_RecommenderSR(Device SR, Device Recommender_SR)
        {
            double trust_SR_Similarity_SR = 0;
            double sum = 0;
            double sim_F = 0, sim_Com = 0, sim_Cen = 0;
            double w_F=0, w_Com=0, w_Cen=0;

            //Calculating w_F, w_L, w_C while w_F + w_L + w_C = 1
            //w_F = 0.33; w_Com = 0.33; w_Cen = 0.33;
            w_F = 1;

            
            ////***** Which we consdier Task Context. if SR and SP have a friend in common, he or she shoud register in the same community group 
            ////***** that shows that freind has same group related to requested task context.
            List<int> list_friendship_Similarity = new List<int>();
            for (int i = 0; i < SR.User.Links.Count(); i++)
            {
                if (Recommender_SR.User.Links.Contains(SR.User.Links[i]))
                {
                    if (list_User[SR.User.Links[i]].GroupCommunities == SR.User.GroupCommunities)
                        list_friendship_Similarity.Add(1);
                }

            }

            //if (list_friendship_Similarity.Count() != 0 && (SR.User.GroupCommunities.Count() * Recommender_SR.User.GroupCommunities.Count()) != 0)
            if (list_friendship_Similarity.Count() != 0 && (SR.User.Links.Count() * Recommender_SR.User.Links.Count()) != 0)
                sim_F = (double)list_friendship_Similarity.Count() / (Math.Sqrt(SR.User.Links.Count() * Recommender_SR.User.Links.Count()));
            else
                sim_F = 0;


            ////***** Calculating Community of Interest Similarity : sim_Com

            List<int> list_GroupComunity_Similarity = new List<int>();
            for (int i = 0; i < SR.User.GroupCommunities.Count(); i++)
            {
                for (int j = 0; j < Recommender_SR.User.GroupCommunities.Count(); j++)
                {
                    if (SR.User.GroupCommunities[i].Equals(Recommender_SR.User.GroupCommunities[j]))
                        list_GroupComunity_Similarity.Add(1);
                }
            }
            if (list_GroupComunity_Similarity.Count() != 0 && (SR.User.GroupCommunities.Count() * Recommender_SR.User.GroupCommunities.Count()) != 0)
                sim_Com = (double)list_GroupComunity_Similarity.Count() / (Math.Sqrt(SR.User.GroupCommunities.Count() * Recommender_SR.User.GroupCommunities.Count()));
            else
                sim_Com = 0;


            //
            trust_SR_Similarity_SR = w_F * sim_F + w_Com * sim_Com + w_Cen * sim_Cen;
            return trust_SR_Similarity_SR;
        }

        /// <summary>
        /// Calculates the similarity between a service requester and a recommender (who is a service provider).
        /// </summary>
        /// <returns>Similarity value as a double.</returns>
        public double SR_Trust_Similarity_RecommenderSP(Device SR, Device Recommender_SP)
        {
            double trust_SR_Similarity_SP = 0;
            double sum = 0;
            double sim_F = 0, sim_Com = 0, sim_Cen = 0;
            double w_F=0, w_Com=0, w_Cen=0;

            w_F = 1;
          
            ////***** Which we consdier Task Context. if SR and SP have a friend in common, he or she shoud register in the same community group 
            ////***** that shows that freind has same group related to requested task context.
            List<int> list_friendship_Similarity = new List<int>();
            for (int i = 0; i < SR.User.Links.Count(); i++)
            {
                if (Recommender_SP.User.Links.Contains(SR.User.Links[i]))
                {
                    if (list_User[SR.User.Links[i]].GroupCommunities == SR.User.GroupCommunities)
                        list_friendship_Similarity.Add(1);
                }
            }

            //if (list_friendship_Similarity.Count() != 0 && (SR.User.GroupCommunities.Count() * Recommender_SP.User.GroupCommunities.Count()) != 0)
            if (list_friendship_Similarity.Count() != 0 && (SR.User.Links.Count() * Recommender_SP.User.Links.Count()) != 0)
                sim_F = (double)list_friendship_Similarity.Count() / (Math.Sqrt(SR.User.Links.Count() * Recommender_SP.User.Links.Count()));
            else
                sim_F = 0;

            ////***** Calculating Community of Interest Similarity : sim_Com

            List<int> list_GroupComunity_Similarity = new List<int>();
            for (int i = 0; i < SR.User.GroupCommunities.Count(); i++)
            {
                for (int j = 0; j < Recommender_SP.User.GroupCommunities.Count(); j++)
                {
                    if (SR.User.GroupCommunities[i].Equals(Recommender_SP.User.GroupCommunities[j]))
                        list_GroupComunity_Similarity.Add(1);
                }
            }

            if (list_GroupComunity_Similarity.Count() != 0 && (SR.User.GroupCommunities.Count() * Recommender_SP.User.GroupCommunities.Count()) != 0)
                sim_Com = (double)list_GroupComunity_Similarity.Count() / (Math.Sqrt(SR.User.GroupCommunities.Count() * Recommender_SP.User.GroupCommunities.Count()));
            else
                sim_Com = 0;

            
            //
            trust_SR_Similarity_SP = w_F * sim_F + w_Com * sim_Com + w_Cen * sim_Cen;
            return trust_SR_Similarity_SP;
        }

        /// <summary>
        /// Calculates the contextual mutuality trust for a recommender (SP) from the perspective of a service requester.
        /// </summary>
        /// <returns>Context_Feedback object with trust value and context info.</returns>
        public Context_Feedback SR_Contextual_Mutuality_Trust__Recommender_SP(Device SR, Device SP)
        {
            double trust_SR_CMT_SP = 0, trust_SR_CMT_QoS_SP = 0, trust_SR_CMT_Social_SP = 0;
            double decay = 1, variation = 1;
            double C = 0.5;
            double norm_Vector_SR = 0, norm_Vector_SP = 0, angle_Vectors = 0, dotproduct = 0;
            ////*****
            Context_Feedback context_SP = new Context_Feedback();
            Context_Feedback context_SR = new Context_Feedback();
            List<Context_Feedback> context_feedbacks = new List<Context_Feedback>();
            Context_Feedback context = new Context_Feedback();
            List<double> feedbacks = new List<double>();
            ////***** Calculation Trust of Standars and Trust of QoS
            //trust_SR_CMT_QoS_SP = SR.Energy * SP.Energy + SR.Computation * SP.Computation + Get_Location_Distance(SR, SP) + Get_Time_Distance(SP, SR) + Get_Profession_Distance(SR, SP);
            ////***** Contexts of SP
            context_SP.Energy = SP.Energy;
            context_SP.Computation = SP.Computation;
            context_SP.Profession = User.GetNumberByProf(SP.User.Profession);
            //context_SP.Location_Service = Unit.GetNumberByUnit(SP.Services.Location_Service);
            //context_SP.Time_Service = Service.GetNumberByTime(SP.Services.Time_Service);
            ////***Computing the workload by time and location
            Random r = new Random();
            //Context_Feedback.WorkLoad WL = Context_Feedback.GetWorkLoadByNumber(r.Next(1, 5));
            //int WL_int = Context_Feedback.GetNumberByWorkLoad(WL);
            //context_SP.Workloads = WL_int;
            //context_SP.Workloads = r.Next(1, 5);
            
            ////***** Context of SR
            context_SR.Energy = SR.Energy;
            context_SR.Computation = SR.Computation;
            context_SR.Profession = User.GetNumberByProf(SR.User.Profession);
            //context_SR.Location_Service = Unit.GetNumberByUnit(SR.Services.Location_Service);
            //context_SR.Time_Service = Service.GetNumberByTime(SR.Services.Time_Service);
            ////*** We assump that the best workload which is computed by time and locatin is equal to 1 for SR
            //context_SR.Workloads = 1;

            //////***** Calculation angle between vectors of service provider and service requesters in the N-dimention Space of Contexst (Environment,Device)
            //norm_Vector_SP = Math.Sqrt(Math.Pow(context_SP.Energy, 2) + Math.Pow(context_SP.Computation, 2) +
            //   Math.Pow(context_SP.Profession, 2) + Math.Pow(context_SP.Location_Service, 2) + Math.Pow(context_SP.Time_Service, 2));

            //norm_Vector_SR = Math.Sqrt(Math.Pow(context_SR.Energy, 2) + Math.Pow(context_SR.Computation, 2) +
            //Math.Pow(context_SR.Profession, 2) + Math.Pow(context_SR.Location_Service, 2) + Math.Pow(context_SR.Time_Service, 2));

            //dotproduct = context_SP.Energy * context_SR.Energy + context_SP.Computation * context_SR.Computation +
            //    context_SP.Profession * context_SR.Profession + context_SP.Location_Service * context_SR.Location_Service + context_SP.Time_Service * context_SR.Time_Service;


            norm_Vector_SP = Math.Sqrt(Math.Pow(context_SP.Energy, 2) + Math.Pow(context_SP.Computation, 2) +
              Math.Pow(context_SP.Profession, 2) + Math.Pow(context_SP.Workloads, 2));

            norm_Vector_SR = Math.Sqrt(Math.Pow(context_SR.Energy, 2) + Math.Pow(context_SR.Computation, 2) +
            Math.Pow(context_SR.Profession, 2) + Math.Pow(context_SR.Workloads, 2));

            dotproduct = context_SP.Energy * context_SR.Energy + context_SP.Computation * context_SR.Computation +
                context_SP.Profession * context_SR.Profession + context_SP.Workloads * context_SR.Workloads;


            ////angle_Vectors = Math.Acos(dotproduct / (norm_Vector_SP * norm_Vector_SR));
            angle_Vectors = dotproduct / (norm_Vector_SP * norm_Vector_SR);
            trust_SR_CMT_QoS_SP = angle_Vectors;

            trust_SR_CMT_Social_SP = SR_Trust_SimilaritySocial_SP(SR, SP);
            double distance = 1 - trust_SR_CMT_Social_SP;


            if (SR.Visited_SP_Feedback.Count() == 0 || (SR.Visited_SP_Feedback.Any(p => p.Key.Id.Equals(SP.Id)) && SR.Visited_SP_Feedback.Single(q => q.Key.Id.Equals(SP.Id)).Value.Count()==0))
            {
                //For first intiraction, trustor should use the recommendation feedback 
                trust_SR_CMT_SP = C * trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance);
            }
            else if (SR.Visited_SP_Feedback.Any(p => p.Key.Id.Equals(SP.Id)) && SR.Visited_SP_Feedback.Single(q => q.Key.Id.Equals(SP.Id)).Value.Count()!=0)
            {
                context_feedbacks = SR.Visited_SP_Feedback.Single(p => p.Key.Id.Equals(SP.Id)).Value;
                feedbacks = context_feedbacks.Select(p => p.Feedback).ToList();
                decay = Get_Decay(feedbacks);
                variation = Get_Variation(feedbacks);
                //trust_SR_CMT_SP = trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance) + decay * (1 / variation) * feedbacks[feedbacks.Count() - 1];
                //trust_SR_CMT_SP = trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance) + Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1];
                trust_SR_CMT_SP = C * trust_SR_CMT_QoS_SP * Math.Pow(Math.E, -distance) + (1-C) * Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1];            
            }

             //then insert the result in the output not in feedback.
            context.Id = SP.Id;
            context.Feedback = trust_SR_CMT_SP;
            context.Location_Service = Unit.GetNumberByUnit(SP.Current_Location);
            context.Time_Service = Service.GetNumberByTime(SP.Services.Time_Service);
            context.Services_Provided = Service.GetNumberByService(SP.Services.Services_Provided);

            return context;
        }

        /// <summary>
        /// Calculates the similarity between the context of a service requester and a recommender's context feedback.
        /// </summary>
        /// <returns>Similarity value as a double.</returns>
        public double SR_Similarity_Contexts_Recommender_SP(Device SR, Context_Feedback Context_Feedback)
        {
            double sim_Contexts = 0, dis_Context = 0;
            double Delta_Loc = 0, Delta_Time = 0, Delta_Service = 0;
            double mean_Loc, mean_Time, mean_Service = 0;
            double variance_Loc, variance_Time, variance_Service = 0;
            double s_deviation_Loc, s_deviation_Time, s_deviatione_Service = 0;

            Delta_Loc = Math.Abs(Unit.GetNumberByUnit(SR.Services.Location_Service) - Context_Feedback.Location_Service);
            mean_Loc = (double) (Unit.GetNumberByUnit(SR.Services.Location_Service) + Context_Feedback.Location_Service) / 2;
            //variance_Loc = (double) (Math.Pow(Unit.GetNumberByUnit(SR.Services.Location_Service) - mean_Loc, 2) + Math.Pow(Context_Feedback.Location_Service - mean_Loc, 2)) / 2;
            //s_deviation_Loc = Math.Sqrt(variance_Loc);

            Delta_Time = Math.Abs(Service.GetNumberByTime(SR.Services.Time_Service) - Context_Feedback.Time_Service);
            mean_Time = (double)(Service.GetNumberByTime(SR.Services.Time_Service) + Context_Feedback.Time_Service) / 2;
            //variance_Time = (double) (Math.Pow(Service.GetNumberByTime(SR.Services.Time_Service) - mean_Time, 2) + Math.Pow(Context_Feedback.Time_Service - mean_Time, 2)) / 2;
            //s_deviation_Time = Math.Sqrt(variance_Time);

            Delta_Service = Math.Abs(Service.GetNumberByService(SR.Services.Services_Requetsed) - Context_Feedback.Services_Provided);
            mean_Service = (double) (Service.GetNumberByService(SR.Services.Services_Requetsed) + Context_Feedback.Services_Provided) / 2;
            //variance_Service = (double) (Math.Pow(Service.GetNumberByService(SR.Services.Services_Requetsed) - mean_Service, 2) + Math.Pow(Context_Feedback.Services_Provided - mean_Service, 2)) / 2;
            //s_deviatione_Service = Math.Sqrt(variance_Service);

            //Delta_Loc = Math.Abs(Unit.GetNumberByUnit(SR.Services.Location_Service) - Context_Feedback.Location_Service);
            //Delta_Time = Math.Abs(Service.GetNumberByTime(SR.Services.Time_Service) - Context_Feedback.Time_Service);
            //Delta_Service = Math.Abs(Service.GetNumberByService(SR.Services.Services_Requetsed) - Context_Feedback.Services_Provided);

            //dis_Context = ((Delta_Loc!=0) ?  Math.Sqrt((double)Math.Pow(Delta_Loc, 2) / Math.Pow(s_deviation_Loc, 2)) :1) + ((Delta_Loc!=0)? (double)Math.Pow(Delta_Time, 2) / Math.Pow(s_deviation_Time, 2) : 1 )+ ((Delta_Service!=0) ? (double) Math.Pow(Delta_Service, 2) / Math.Pow(s_deviatione_Service, 2) : 1);
            dis_Context = Math.Sqrt(Math.Pow(Delta_Loc, 2) +Math.Pow(Delta_Time, 2) +Math.Pow(Delta_Service, 2));

            double max = Math.Sqrt(Math.Pow(Unit.GetNumberByUnit(Unit.Type.P1) - Unit.GetNumberByUnit(Unit.Type.P10), 2) + Math.Pow(Service.GetNumberByTime(Service.Time.Next_Month) - Service.GetNumberByTime(Service.Time.Last_Years), 2) + Math.Pow(Service.GetNumberByService(Service.Type.S1) - Service.GetNumberByService(Service.Type.S5), 2));
            double min = Math.Sqrt(Math.Pow(Unit.GetNumberByUnit(Unit.Type.P1) - Unit.GetNumberByUnit(Unit.Type.P1), 2) + Math.Pow(Service.GetNumberByTime(Service.Time.Next_Month) - Service.GetNumberByTime(Service.Time.Next_Month), 2) + Math.Pow(Service.GetNumberByService(Service.Type.S1) - Service.GetNumberByService(Service.Type.S1), 2));
            dis_Context = (double) (dis_Context - min) / (max - min);
            sim_Contexts = 1 - dis_Context;
            //sim_Contexts = Math.Pow(Math.E, -dis_Context);
            return sim_Contexts;
        }

        /// <summary>
        /// Placeholder for learning based on context similarity (not implemented).
        /// </summary>
        public void Similarity_Contexts_Learning(Device SR, Device SP)
        {

        }

        //***************//
        /// <summary>
        /// Selects the most trustworthy service provider for a service requester based on computed trust values.
        /// </summary>
        /// <returns>Dictionary with selected device and its trust value.</returns>
        public Dictionary<Device, double> SR_Decision(Device SR)
        {
            Dictionary<Device, double> selected_SP = new Dictionary<Device, double>();
            Device dev = new Device();
            double trust = 0;

            if (SR.Trusthworthiness_SP.Where(p => p.Value != 0).Count() != 0)
            {
                //double selected_SP2 = SR.Trusthworthiness_SP.OrderBy(p => p.Value).ToList().First().Single().Value;
                dev = SR.Trusthworthiness_SP.Where(p => p.Value != 0).OrderByDescending(p => p.Value).First().Key;
                trust = SR.Trusthworthiness_SP.Where(p => p.Key.Id.Equals(dev.Id)).OrderByDescending(p => p.Value).First().Value;
              
            }
            else
            {
                dev = SR.Trusthworthiness_SP.First().Key;

                trust = 0;
            }

            selected_SP.Add(dev, trust);

            return selected_SP;
        }

        /// <summary>
        /// Sends a transaction request from a service requester to a service provider.
        /// </summary>
        public void SR_Send_Transaction(Device SR, Device SP)
        {
            list_Dev.Single(p => p.Id.Equals(SP.Id)).Potential_SR.Add(SR,0);
        }

        /// <summary>
        /// Performs post-evaluation for a service requester after a transaction, updating feedback and satisfaction.
        /// </summary>
        public void SR_PostEvaluation(Device SR, Dictionary<Device, double> selected_SP)
        {
            //Feedback Value
            Random r = new Random();
            Dictionary<Device, double[]> dictionary_Feedback = new Dictionary<Device, double[]>();
            int T, T1, T2, T3 = 0;
            int time_Real, time_Expected, service_Real, service_Expected, link_Before, link_After;
            double gain = 0, loss = 0;
            double Delta, Delta_Time, Delta_Service, Delta_Link;
            //double A = 1, B = 1, a = 0.02, b = 0.02, x = 2;
            double A = 1, B = 1, a = 0.4, b = 0.6, x = 2;
            double feedback;
            double feedback_last;
            Context_Feedback context_feedback = new Context_Feedback();
            Device SP = selected_SP.Single().Key;
            double trust = selected_SP.Single().Value;

            if (SP.Services.Time_Response.Equals(Service.TimeResponse.bad))
                T1 = 1; //Bad//Penalty
            else
                T1 = 0; //Good//Reward

            //Suppose: the expected time respinse is good=1
            Delta_Time = Math.Abs(Service.GetNumberByTimeResponse(SR.Services.Time_Response) - Service.GetNumberByTimeResponse(SP.Services.Time_Response));

            ///////
            if (SP.Services.OoS.Equals(Service.QoS.bad))
                T2 = 1;//Bad//Penalty
            else
                T2 = 0;//Good//Reward

            Delta_Service = Math.Abs(Service.GetNumberByQoS(SR.Services.OoS) - Service.GetNumberByQoS(SP.Services.OoS));

            if (SP.ground_Trust > 0.55 && SP.ground_Trust< 0.60)
                T3 = 1;//Bad//Penalty
            else
                T3 = 0;//Good//Reward

            /////***** Satisfication
            int satisfication = 3; //good
            if (T1 == 0 && T2 == 0)
                satisfication = 3;//good
            else if ((T1 == 1 && T2 == 0) || (T1 == 0 && T2 == 1))
                satisfication = 2;//so so
            else if (T1 == 1 && T2 == 1)
                satisfication = 1;//bad
            SR.Satisfication = satisfication;

            
            //////////Without Learning
            A = SP.ground_Trust;
            B = 1 - SP.ground_Trust;

            //feedback = ((double)A / (A + B));
            //feedback = ((double)A / (A + B)) * trust;
            feedback = A * trust;
            /////////


            context_feedback.Id = SP.Id;
            context_feedback.Feedback = feedback;
            context_feedback.Location_Service = Unit.GetNumberByUnit(SP.Current_Location);
            context_feedback.Time_Service = Service.GetNumberByTime(SP.Services.Time_Service);
            context_feedback.Services_Provided = Service.GetNumberByService(SP.Services.Services_Provided);
            //context_feedback.Services_Requetsed.Add(SR.Services.Services_Requetsed, Service.GetNumberByService(SP.Services.Services_Requetsed));

            //if (SR.Visited_SP_Feedback.Count() == 0 || SR.Visited_SP_Feedback.Single(p=>p.Key.Id.Equals(SP.Id)).Value.Count() == 0)
            if (SR.Visited_SP_Feedback.Count() == 0 || (SR.Visited_SP_Feedback.Any(p => p.Key.Id.Equals(SP.Id) && SR.Visited_SP_Feedback.Single(q => q.Key.Id.Equals(SP.Id)).Value.Count() == 0)))
            {
                SR.Visited_SP_Feedback.Add(SP, new List<Context_Feedback> { context_feedback });
            }
            else if (SR.Visited_SP_Feedback.Any(p => p.Key.Id.Equals(SP.Id) && SR.Visited_SP_Feedback.Single(q => q.Key.Id.Equals(SP.Id)).Value.Count() != 0))
            {

                SR.Visited_SP_Feedback.Single(p => p.Key.Id.Equals(SP.Id)).Value.Add(context_feedback);
            }
            //}

        }

        /// <summary>
        /// Placeholder for moving a service requester to a new location (not implemented).
        /// </summary>
        public void SR_Move(Device SR)
        {
        }

        //************************************************SERVICE PROVIDER*************************************************************//
        /// <summary>
        /// Pre_Evalution from SP to SR
        /// Then, Get Decision by SP to arrange the most trustworthiness SR by their trust value 
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public void SP_PreEvaluation(Device SP)
        {
            ////***** 1. recieve many service requests from different service requesters 
            List<Device> R = new List<Device>();
            R.AddRange(SP.Potential_SR.Select(q => q.Key).ToList<Device>());

            ////***** 2. computing trust_value for each SR to have potential service requesters
            foreach (Device SR in R)
            {
                double trust_SP_SR = 0;
                trust_SP_SR = SP_TrustComputing_SR(SP, SR);
                SP.Potential_SR.Remove(SR);
                Dictionary<Device, double> sp = new Dictionary<Device, double>();
                SP.Potential_SR.Add(SR,trust_SP_SR);
            }

            SP.Trusthworthiness_SR = SP.Potential_SR;
        }

        /// <summary>
        /// Computes the trust value from a service provider to a service requester.
        /// </summary>
        /// <returns>Trust value as a double.</returns>
        public double SP_TrustComputing_SR(Device SP, Device SR)
        {
            //double A = 0.7;
            //double B = 0.1;
            double A = 0.8;
            double trust_SP_SR = 0;
            //trust_SP_SR = A * SP_Contextual_Mutuality_Trust_SR(SP, SR) + (1-A) * SP_Contextual_Social_Trust_SR(SP, SR);
            trust_SP_SR = A * SP_Contextual_Mutuality_Trust_SR(SP, SR) + (1-A) * SP_Contextual_Social_Trust_SR(SP, SR);
            return trust_SP_SR;
        }

        //***************//
        /// <summary>
        /// Calculates the contextual mutuality trust between a service provider and requester based on context vectors and feedback.
        /// </summary>
        /// <returns>Mutuality trust value as a double.</returns>
        public double SP_Contextual_Mutuality_Trust_SR(Device SP, Device SR)
        {
            double trust_SP_CMT_SR = 0, trust_SP_CMT_QoS_SR = 0;
            double decay = 1, variation = 1;
            double C = 0.5;
            double norm_Vector_SR = 0, norm_Vector_SP = 0, angle_Vectors = 0, dotproduct = 0;
            ////*****
            Context_Feedback context_SP = new Context_Feedback();
            Context_Feedback context_SR = new Context_Feedback();
            List<Context_Feedback> context_feedbacks = new List<Context_Feedback>();
            List<double> feedbacks = new List<double>();
            ////***** Calculation Trust of Standars and Trust of QoS
            // trust_SP_CMT_QoS_SR = SP.Energy * SR.Energy + SP.Computation * SR.Computation + Get_Location_Distance(SP, SR) + Get_Time_Distance(SP, SR) + Get_Profession_Distance(SP, SR);

            ////***** Contexts of SP
            context_SP.Energy = SP.Energy;
            context_SP.Computation = SP.Computation;
            context_SP.Profession = User.GetNumberByProf(SP.User.Profession);
            //context_SP.Location_Service = Unit.GetNumberByUnit(SP.Services.Location_Service);
            //context_SP.Time_Service = Service.GetNumberByTime(SP.Services.Time_Service);
            ////***Computing the workload by time and location
            Random r = new Random();
            //Context_Feedback.WorkLoad WL = Context_Feedback.GetWorkLoadByNumber(r.Next(1, 5));
            //int WL_int = Context_Feedback.GetNumberByWorkLoad(WL);
            //context_SP.Workloads = WL_int;
            //context_SP.Workloads = r.Next(1, 5);

            ////***** Context of SR
            context_SR.Energy = SR.Energy;
            context_SR.Computation = SR.Computation;
            context_SR.Profession = User.GetNumberByProf(SR.User.Profession);
            //context_SR.Location_Service = Unit.GetNumberByUnit(SR.Services.Location_Service);
            //context_SR.Time_Service = Service.GetNumberByTime(SR.Services.Time_Service);
            ////*** We assump that the best workload which is computed by time and locatin is equal to 1 for SR
            //context_SR.Workloads = 1;

            ////***** Calculation angle between vectors of service provider and service requesters in the N-dimention Space of Contexst (Environment,Device)
            //norm_Vector_SP = Math.Sqrt(Math.Pow(context_SP.Energy, 2) + Math.Pow(context_SP.Computation, 2) +
            //   Math.Pow(context_SP.Profession, 2) + Math.Pow(context_SP.Location_Service, 2) + Math.Pow(context_SP.Time_Service, 2));

            //norm_Vector_SR = Math.Sqrt(Math.Pow(context_SR.Energy, 2) + Math.Pow(context_SR.Computation, 2) +
            //Math.Pow(context_SR.Profession, 2) + Math.Pow(context_SR.Location_Service, 2) + Math.Pow(context_SR.Time_Service, 2));

            //dotproduct = context_SP.Energy * context_SR.Energy + context_SP.Computation * context_SR.Computation +
            //    context_SP.Profession * context_SR.Profession + context_SP.Location_Service * context_SR.Location_Service + context_SP.Time_Service * context_SR.Time_Service;

            norm_Vector_SP = Math.Sqrt(Math.Pow(context_SP.Energy, 2) + Math.Pow(context_SP.Computation, 2) +
          Math.Pow(context_SP.Profession, 2) + Math.Pow(context_SP.Workloads, 2));

            norm_Vector_SR = Math.Sqrt(Math.Pow(context_SR.Energy, 2) + Math.Pow(context_SR.Computation, 2) +
            Math.Pow(context_SR.Profession, 2) + Math.Pow(context_SR.Workloads, 2));

            dotproduct = context_SP.Energy * context_SR.Energy + context_SP.Computation * context_SR.Computation +
                context_SP.Profession * context_SR.Profession + context_SP.Workloads * context_SR.Workloads;

            ////angle_Vectors = Math.Acos(dotproduct / (norm_Vector_SP * norm_Vector_SR));
            angle_Vectors = dotproduct / (norm_Vector_SP * norm_Vector_SR);
            trust_SP_CMT_QoS_SR = angle_Vectors;

            //if (SP.Visited_SR_Feedback.Count() == 0 || (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Id) && SP.Visited_SR_Feedback.Single(q => q.Key.Id.Equals(SR.Id)).Value.Count()==0)))
            if (SP.Visited_SR_Feedback.Count() == 0)
            {
                trust_SP_CMT_SR = C * trust_SP_CMT_QoS_SR;
            }
            else if (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Id) && SP.Visited_SR_Feedback.Single(q => q.Key.Id.Equals(SR.Id)).Value.Count() == 0))
            {
                trust_SP_CMT_SR = C * trust_SP_CMT_QoS_SR;
            }
            else if (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Id) && SP.Visited_SR_Feedback.Single(q => q.Key.Id.Equals(SR.Id)).Value.Count() != 0))
            {
                context_feedbacks = SP.Visited_SR_Feedback.Single(p => p.Key.Id.Equals(SR.Id)).Value;
                feedbacks = context_feedbacks.Select(p => p.Feedback).ToList();
                decay = Get_Decay(feedbacks);
                variation = Get_Variation(feedbacks);

                //trust_SP_CMT_SR = trust_SP_CMT_QoS_SR + decay *  variation * feedbacks[feedbacks.Count() - 1];
                //trust_SP_CMT_SR = trust_SP_CMT_QoS_SR + Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1];
                trust_SP_CMT_SR = C * trust_SP_CMT_QoS_SR + (1-C) * Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1];
            }
            return trust_SP_CMT_SR;
        }

        /// <summary>
        /// Calculates the contextual social trust from a service provider to a requester using recommendations from friends.
        /// </summary>
        /// <returns>Contextual social trust value as a double.</returns>
        public double SP_Contextual_Social_Trust_SR(Device SP, Device SR)
        {
            double trust_SP_Sum_Recommends_SR = 0;
            double similarity = 0, sum_sum_Similarity = 0, sum_Similarity_Contexts_Recommender = 0;
            double trust_direct_recommender = 0, SR_Similarity_Contexts_Recommender_ = 0, SR_Similarity_Contexts_Recommender=0;
            double distance=0;
            Context_Feedback Context_Feedback = new DomainObjects.Context_Feedback();
            Dictionary<Device, double> Dic_SimSP = new Dictionary<Device, double>();
            Dictionary<Device, double> Dic_SimSR = new Dictionary<Device, double>();
            Dictionary<int, double> list_Similarity = new Dictionary<int, double>();

            
            //step1: select friends with highest similarity
            //Step1-1: compute the sum social similarity trust
            for (int i = 0; i < SP.User.Links.Count(); i++)
            {
                int id_Recommender = SP.User.Links[i];

                if (list_SP.Exists(p => p.Id.Equals(id_Recommender)))
                {
                    Context_Feedback = SP_Contextual_Mutuality_Trust__Recommender_SR(list_SP.Single(p => p.Id.Equals(id_Recommender)), SR);
                }
                else
                {
                    Context_Feedback = SP_Contextual_Mutuality_Trust__Recommender_SR(list_SR.Single(p => p.Id.Equals(id_Recommender)), SR);
                }

                SR_Similarity_Contexts_Recommender = SP_Similarity_Contexts_Recommender_SR(SP, Context_Feedback);
                list_Similarity.Add(id_Recommender, SR_Similarity_Contexts_Recommender);
                sum_Similarity_Contexts_Recommender += SR_Similarity_Contexts_Recommender;
            }

            //step2: select trust value recomendation of selected friends
            for (int i = 0; i < SP.User.Links.Count(); i++)
            {
                int id_Recommender = SP.User.Links[i];
                if (list_SP.Exists(p => p.Id.Equals(id_Recommender)))
                {
                    //distance= 1- Dic_SimSP.Single(p => p.Key.Id.Equals(id_Recommender)).Value;

                    //(Trust value Service Recommender to Service Provider) = previous feedbacks of service recommender to service provider. if it dosen't have feed back, mutuality trust is calculated.
                    Context_Feedback = SP_Contextual_Mutuality_Trust__Recommender_SR(list_SP.Single(p => p.Id.Equals(id_Recommender)), SR);
                    //trust Service Requester to Service Recommender = (similarity Service Requester with Service Recommender as Weight) * (Trust value Service Recommender to Service Provider) * similarity context of Service Requster to context of Service Recommenders.
                    //trust_direct_recommender = Context_Feedback.Feedback * Math.Pow(Math.E, -distance) * SP_Similarity_Contexts_Recommender_SR(list_SP.Single(p => p.Id.Equals(id_Recommender)), Context_Feedback);
                    //trust_direct_recommender = Context_Feedback.Feedback * SP_Similarity_Contexts_Recommender_SR(SP, Context_Feedback);
                    if (sum_Similarity_Contexts_Recommender == 0)
                    {
                        SR_Similarity_Contexts_Recommender_ = 0;
                    }
                    else
                    {
                        SR_Similarity_Contexts_Recommender_ = (double)(list_Similarity.Single(p => p.Key.Equals(id_Recommender)).Value) / (sum_Similarity_Contexts_Recommender);
                    }
                    trust_direct_recommender = Context_Feedback.Feedback * SR_Similarity_Contexts_Recommender_;
                }
                else
                {
                    //distance=1-Dic_SimSR.Single(p => p.Key.Id.Equals(id_Recommender)).Value;
                    // (Trust value Service Recommender to Service Provider) = previous feedbacks of service recommender to service provider. if it dosen't have feed back, mutuality trust is calculated.
                    Context_Feedback = SP_Contextual_Mutuality_Trust__Recommender_SR(list_SR.Single(p => p.Id.Equals(id_Recommender)), SR);
                    //trust Service Requester to Service Recommender = (similarity Service Requester with Service Recommender as Weight) * (Trust value Service Recommender to Service Provider) * similarity context of Service Requster to context of Service Recommenders.
                    //trust_direct_recommender = Context_Feedback.Feedback * Math.Pow(Math.E, -distance) * SP_Similarity_Contexts_Recommender_SR(list_SR.Single(p => p.Id.Equals(id_Recommender)), Context_Feedback);
                    //trust_direct_recommender = Context_Feedback.Feedback * Math.Pow(Math.E, -distance) * SP_Similarity_Contexts_Recommender_SR(SP, Context_Feedback);
                    //trust_direct_recommender = Context_Feedback.Feedback * SP_Similarity_Contexts_Recommender_SR(SP, Context_Feedback);
                    if (sum_Similarity_Contexts_Recommender == 0)
                    {
                        SR_Similarity_Contexts_Recommender_ = 0;
                    }
                    else
                    {
                        SR_Similarity_Contexts_Recommender_ = (double)(list_Similarity.Single(p => p.Key.Equals(id_Recommender)).Value) / (sum_Similarity_Contexts_Recommender);
                    }
                    
                    trust_direct_recommender = Context_Feedback.Feedback * SR_Similarity_Contexts_Recommender_;
                }

                sum_sum_Similarity += trust_direct_recommender;
            }
            
            trust_SP_Sum_Recommends_SR = sum_sum_Similarity;
            return trust_SP_Sum_Recommends_SR;
        }

        /// <summary>
        /// Calculates the similarity between a service provider and a recommender (who is a service requester).
        /// </summary>
        /// <returns>Similarity value as a double.</returns>
        public double SP_Trust_Similarity_RecommenderSR(Device SP, Device Recommender_SR)
        {
            double trust_SP_Similarity_SR = 0;
            double sum = 0;
            double sim_F = 0, sim_Com = 0, sim_Cen = 0;
            double w_F=0, w_Com=0, w_Cen=0;

            //Calculating w_F, w_L, w_C while w_F + w_L + w_C = 1
            //w_F = 0.33; w_Com = 0.33; w_Cen = 0.33;
            w_F = 1;

            ////***** Which we consdier Task Context. if SR and SP have a friend in common, he or she shoud register in the same community group 
            ////***** that shows that freind has same group related to requested task context.
            List<int> list_friendship_Similarity = new List<int>();
            for (int i = 0; i < SP.User.Links.Count(); i++)
            {
                if (Recommender_SR.User.Links.Contains(SP.User.Links[i]))
                {
                    //if (list_User[SP.User.Links[i]].GroupCommunities == SP.User.GroupCommunities)
                        list_friendship_Similarity.Add(1);
                }
            }

            //if (list_friendship_Similarity.Count() != 0 && (SP.User.GroupCommunities.Count() * Recommender_SR.User.GroupCommunities.Count()) != 0)
            if (list_friendship_Similarity.Count() != 0 && (SP.User.Links.Count() * Recommender_SR.User.Links.Count()) != 0)
                sim_F = (double)list_friendship_Similarity.Count() / (Math.Sqrt(SP.User.Links.Count() * Recommender_SR.User.Links.Count()));
            else
                sim_F = 0;

           
            ////***** Calculating Community of Interest Similarity : sim_Com

            List<int> list_GroupComunity_Similarity = new List<int>();
            for (int i = 0; i < SP.User.GroupCommunities.Count(); i++)
            {
                for (int j = 0; j < Recommender_SR.User.GroupCommunities.Count(); j++)
                {
                    //if (SP.User.GroupCommunities[i].Equals(Recommender_SR.User.GroupCommunities[j]))
                        list_GroupComunity_Similarity.Add(1);
                }
            }

            if (list_GroupComunity_Similarity.Count() != 0 && (SP.User.GroupCommunities.Count() * Recommender_SR.User.GroupCommunities.Count()) != 0)
                sim_Com = (double)list_GroupComunity_Similarity.Count() / (Math.Sqrt(SP.User.GroupCommunities.Count() * Recommender_SR.User.GroupCommunities.Count()));
            else
                sim_Com = 0;


            //
            trust_SP_Similarity_SR = w_F * sim_F + w_Com * sim_Com + w_Cen * sim_Cen;
            return trust_SP_Similarity_SR;
        }

        /// <summary>
        /// Calculates the similarity between a service provider and a recommender (who is a service provider).
        /// </summary>
        /// <returns>Similarity value as a double.</returns>
        public double SP_Trust_Similarity_RecommenderSP(Device SP, Device Recommender_SP)
        {
            double trust_SP_Similarity_SP = 0;
            double sum = 0;
            double sim_F = 0, sim_Com = 0, sim_Cen = 0;
            double w_F=0, w_Com=0, w_Cen=0;


            //Calculating w_F, w_L, w_C while w_F + w_L + w_C = 1
            //w_F = 0.33; w_Com = 0.33; w_Cen = 0.33;
            w_F = 1; 
            
            List<int> list_friendship_Similarity = new List<int>();
            for (int i = 0; i < SP.User.Links.Count(); i++)
            {
                if (Recommender_SP.User.Links.Contains(SP.User.Links[i]))
                {
                    //if (list_User[SP.User.Links[i]].GroupCommunities == SP.User.GroupCommunities)
                        list_friendship_Similarity.Add(1);
                }

            }

            //if (list_friendship_Similarity.Count() != 0 && (SP.User.GroupCommunities.Count() * Recommender_SP.User.GroupCommunities.Count()) != 0)
            if (list_friendship_Similarity.Count() != 0 && (SP.User.Links.Count() * Recommender_SP.User.Links.Count()) != 0)
                sim_F = (double)list_friendship_Similarity.Count() / (Math.Sqrt(SP.User.Links.Count() * Recommender_SP.User.Links.Count()));
            else
                sim_F = 0;


            ////***** Calculating Community of Interest Similarity : sim_Com

            List<int> list_GroupComunity_Similarity = new List<int>();
            for (int i = 0; i < SP.User.GroupCommunities.Count(); i++)
            {
                for (int j = 0; j < Recommender_SP.User.GroupCommunities.Count(); j++)
                {
                    //if (SP.User.GroupCommunities[i].Equals(Recommender_SP.User.GroupCommunities[j]))
                        list_GroupComunity_Similarity.Add(1);
                }
            }

            if (list_GroupComunity_Similarity.Count() != 0 && (SP.User.GroupCommunities.Count() * Recommender_SP.User.GroupCommunities.Count()) != 0)
                sim_Com = (double)list_GroupComunity_Similarity.Count() / (Math.Sqrt(SP.User.GroupCommunities.Count() * Recommender_SP.User.GroupCommunities.Count()));
            else
                sim_Com = 0;

            trust_SP_Similarity_SP = w_F * sim_F + w_Com * sim_Com + w_Cen * sim_Cen;
            return trust_SP_Similarity_SP;
        }

        /// <summary>
        /// Calculates the contextual mutuality trust for a recommender (SR) from the perspective of a service provider.
        /// </summary>
        /// <returns>Context_Feedback object with trust value and context info.</returns>
        public Context_Feedback SP_Contextual_Mutuality_Trust__Recommender_SR(Device SP, Device SR)
        {
            double trust_SP_CMT_SR = 0, trust_SP_CMT_QoS_SR = 0;
            double decay = 1, variation = 1;
            double C = 0.5;
            double norm_Vector_SR = 0, norm_Vector_SP = 0, angle_Vectors = 0, dotproduct = 0;
            ////*****
            Context_Feedback context_SP = new Context_Feedback();
            Context_Feedback context_SR = new Context_Feedback();
            List<Context_Feedback> context_feedbacks = new List<Context_Feedback>();
            Context_Feedback context = new Context_Feedback();
            List<double> feedbacks = new List<double>();
            ////***** Calculation Trust of Standars and Trust of QoS
            //trust_SP_CMT_QoS_SR = SR.Energy * SP.Energy + SR.Computation * SP.Computation + Get_Location_Distance(SR, SP) + Get_Time_Distance(SP, SR) + Get_Profession_Distance(SR, SP);

            ////***** Contexts of SP
            context_SP.Energy = SP.Energy;
            context_SP.Computation = SP.Computation;
            context_SP.Profession = User.GetNumberByProf(SP.User.Profession);
            //context_SP.Location_Service = Unit.GetNumberByUnit(SP.Services.Location_Service);
            //context_SP.Time_Service = Service.GetNumberByTime(SP.Services.Time_Service);
            ////***** Context of SR
            context_SR.Energy = SR.Energy;
            context_SR.Computation = SR.Computation;
            context_SR.Profession = User.GetNumberByProf(SR.User.Profession);
            //context_SR.Location_Service = Unit.GetNumberByUnit(SR.Services.Location_Service);
            //context_SR.Time_Service = Service.GetNumberByTime(SR.Services.Time_Service);

            ////***** Calculation angle between vectors of service provider and service requesters in the N-dimention Space of Contexst (Environment,Device)
            norm_Vector_SP = Math.Sqrt(Math.Pow(context_SP.Energy, 2) + Math.Pow(context_SP.Computation, 2) +
               Math.Pow(context_SP.Profession, 2) + Math.Pow(context_SP.Location_Service, 2) + Math.Pow(context_SP.Time_Service, 2));

            norm_Vector_SR = Math.Sqrt(Math.Pow(context_SR.Energy, 2) + Math.Pow(context_SR.Computation, 2) +
            Math.Pow(context_SR.Profession, 2) + Math.Pow(context_SR.Location_Service, 2) + Math.Pow(context_SR.Time_Service, 2));

            dotproduct = context_SP.Energy * context_SR.Energy + context_SP.Computation * context_SR.Computation +
                context_SP.Profession * context_SR.Profession + context_SP.Location_Service * context_SR.Location_Service + context_SP.Time_Service * context_SR.Time_Service;

            ////angle_Vectors = Math.Acos(dotproduct / (norm_Vector_SP * norm_Vector_SR));
            angle_Vectors = dotproduct / (norm_Vector_SP * norm_Vector_SR);
            trust_SP_CMT_QoS_SR = angle_Vectors;

            
            //if (SP.Visited_SR_Feedback.Count() == 0 || (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Id) && SP.Visited_SR_Feedback.Single(q => q.Key.Id.Equals(SR.Id)).Value.Count()==0)))
            if (SP.Visited_SR_Feedback.Count() == 0)
            {
                trust_SP_CMT_SR = C * trust_SP_CMT_QoS_SR;
            }
            else if (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Id) && SP.Visited_SR_Feedback.Single(q => q.Key.Id.Equals(SR.Id)).Value.Count() == 0))
            {
                trust_SP_CMT_SR = C * trust_SP_CMT_QoS_SR;
            }
            else if (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Id) && SP.Visited_SR_Feedback.Single(q => q.Key.Id.Equals(SR.Id)).Value.Count() != 0))
            {
                context_feedbacks = SP.Visited_SR_Feedback.Single(p => p.Key.Id.Equals(SR.Id)).Value;
                feedbacks = context_feedbacks.Select(p => p.Feedback).ToList();
                decay = Get_Decay(feedbacks);
                variation = Get_Variation(feedbacks);
                //trust_SP_CMT_SR = trust_SP_CMT_QoS_SR + decay * variation * feedbacks[feedbacks.Count() - 1];
                //trust_SP_CMT_SR = trust_SP_CMT_QoS_SR + decay * Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1];
                trust_SP_CMT_SR = C * trust_SP_CMT_QoS_SR + (1-C) * Math.Pow(Math.E, -variation) * feedbacks[feedbacks.Count() - 1] ;

            }
            
            //then insert the result in the output not in feedback.
            context.Id = SR.Id;
            context.Feedback = trust_SP_CMT_SR;
            context.Location_Service = Unit.GetNumberByUnit(SR.Current_Location);
            context.Time_Service = Service.GetNumberByTime(SR.Services.Time_Service);
            context.Services_Provided = Service.GetNumberByService(SR.Services.Services_Provided);
            return context;
        }

        /// <summary>
        /// Calculates the social similarity trust between a service provider and a service requester based on friends and communities.
        /// </summary>
        /// <returns>Social similarity trust value as a double.</returns>
        public double SP_Trust_SimilaritySocial_SR(Device SP, Device SR)
        {
            double trust_SR_Similarity_SP = 0;
            double sum = 0;
            double sim_F = 0, sim_Com = 0, sim_Cen = 0;
            double w_F=0, w_Com=0, w_Cen=0;

            //Calculating w_F, w_L, w_C while w_F + w_L + w_C = 1
            //w_F = 0.33; w_Com = 0.33; w_Cen = 0.33;
            w_F = 1;

            ////***** Which we consdier Task Context. if SR and SP have a friend in common, he or she shoud register in the same community group 
            ////***** that shows that freind has same group related to requested task context.
            List<int> list_friendship_Similarity = new List<int>();
            for (int i = 0; i < SP.User.Links.Count(); i++)
            {
                if (SR.User.Links.Contains(SP.User.Links[i]))
                {
                    //if (list_User[SP.User.Links[i]].GroupCommunities == SP.User.GroupCommunities)
                        list_friendship_Similarity.Add(1);
                }

            }

            //if (list_friendship_Similarity.Count() != 0 && (SR.User.GroupCommunities.Count() * SP.User.GroupCommunities.Count()) != 0)
            if (list_friendship_Similarity.Count() != 0 && (SP.User.Links.Count() * SR.User.Links.Count()) != 0)
                sim_F = (double)list_friendship_Similarity.Count() / (Math.Sqrt(SP.User.Links.Count() * SR.User.Links.Count()));
            else
                sim_F = 0;

            
            ////***** Calculating Community of Interest Similarity : sim_Com

            List<int> list_GroupComunity_Similarity = new List<int>();
            for (int i = 0; i < SP.User.GroupCommunities.Count(); i++)
            {
                for (int j = 0; j < SR.User.GroupCommunities.Count(); j++)
                {
                    //if (SP.User.GroupCommunities[i].Equals(SR.User.GroupCommunities[j]))
                        list_GroupComunity_Similarity.Add(1);
                }
            }

            if (list_GroupComunity_Similarity.Count() != 0 && (SR.User.GroupCommunities.Count() * SP.User.GroupCommunities.Count()) != 0)
                sim_Com = (double)list_GroupComunity_Similarity.Count() / (Math.Sqrt(SP.User.GroupCommunities.Count() * SR.User.GroupCommunities.Count()));
            else
                sim_Com = 0;

           
            //
            trust_SR_Similarity_SP = w_F * sim_F + w_Com * sim_Com + w_Cen * sim_Cen;
            return trust_SR_Similarity_SP;
        }

        /// <summary>
        /// Calculates the similarity between the context of a service requester and a recommender's context feedback (from SP's perspective).
        /// </summary>
        /// <returns>Similarity value as a double.</returns>
        public double SP_Similarity_Contexts_Recommender_SR(Device SR, Context_Feedback Context_Feedback)
        {
            double sim_Contexts = 0, dis_Context = 0;
            double Delta_Loc = 0, Delta_Time = 0, Delta_Service = 0;
            Delta_Loc =  Math.Abs(Unit.GetNumberByUnit(SR.Services.Location_Service) - Context_Feedback.Location_Service);
            Delta_Time =  Math.Abs(Service.GetNumberByTime(SR.Services.Time_Service) - Context_Feedback.Time_Service);
            Delta_Service =  Math.Abs(Service.GetNumberByService(SR.Services.Services_Requetsed) - Context_Feedback.Services_Provided);
            dis_Context = Math.Sqrt(Math.Pow(Delta_Loc, 2) + Math.Pow(Delta_Time, 2) + Math.Pow(Delta_Service, 2));

            double max = Math.Sqrt(Math.Pow(Unit.GetNumberByUnit(Unit.Type.P1) - Unit.GetNumberByUnit(Unit.Type.P10), 2) + Math.Pow(Service.GetNumberByTime(Service.Time.Next_Month) - Service.GetNumberByTime(Service.Time.Last_Years), 2) + Math.Pow(Service.GetNumberByService(Service.Type.S1) - Service.GetNumberByService(Service.Type.S5), 2));
            double min = Math.Sqrt(Math.Pow(Unit.GetNumberByUnit(Unit.Type.P1) - Unit.GetNumberByUnit(Unit.Type.P1), 2) + Math.Pow(Service.GetNumberByTime(Service.Time.Next_Month) - Service.GetNumberByTime(Service.Time.Next_Month), 2) + Math.Pow(Service.GetNumberByService(Service.Type.S1) - Service.GetNumberByService(Service.Type.S1), 2));
            dis_Context = (double)(dis_Context - min) / (max - min);
            
            sim_Contexts = 1 - dis_Context;
            //sim_Contexts = Math.Pow(Math.E, -dis_Context);
            return sim_Contexts;
        }

        /// <summary>
        /// Selects the most trustworthy service requester for a service provider based on computed trust values.
        /// </summary>
        /// <returns>Dictionary with selected device and its trust value.</returns>
        public Dictionary<Device, double> SP_Decision(Device SP)
        {
            Dictionary<Device, double> selected_SR = new Dictionary<Device, double>();
            
            //double selected_SP2 = SR.Trusthworthiness_SP.OrderBy(p => p.Value).ToList().First().Single().Value;

            if (SP.Trusthworthiness_SR.Count() != 0)
            {
                Device dev = SP.Trusthworthiness_SR.OrderBy(p => p.Value).First().Key;
                double trust = SP.Trusthworthiness_SR.OrderBy(p => p.Value).First().Value;
                selected_SR.Add(dev, trust);
                list_SR.Single(p => p.Id.Equals(dev.Id)).SR_IsTaskDone = true;
            }

            foreach (var SR in SP.Trusthworthiness_SR)
            {
                SP.Trusthworthiness_SR.Where(p => p.Key.Id.Equals(SR.Key.Id)).Single().Key.SR_IsTaskDone = true;
                list_SR.Single(p => p.Id.Equals(SR.Key.Id)).SR_IsTaskDone = true;
            }

            return selected_SR;
        }

        /// <summary>
        /// Placeholder for service provider transaction logic (not implemented).
        /// </summary>
        public void SP_Transaction(Device SP)
        {

        }

        /// <summary>
        /// Performs post-evaluation for a service provider after a transaction, updating feedback for the selected service requester.
        /// </summary>
        public void SP_PostEvaluation(Device SP, Dictionary<Device, double> selected_SR)
        {
            //Feedback Value
            Random r = new Random();
            Dictionary<Device, double[]> dictionary_Feedback = new Dictionary<Device, double[]>();
            double positive = 0, negative = 0;
            int T, T1, T2, T3 = 0;
            int time_Real, time_Expected, service_Real, service_Expected, link_Before, link_After;
            double Delta, Delta_Time, Delta_Resource, Delta_Link;
            //double A = 1, B = 1, a = 0.02, b = 0.02, x = 2;
            //double A = 1, B = 1, a = 0.9, b = 0.1, x = 2;
            double A = 1, B = 1, a = 0.4, b = 0.6, x = 2;
            double feedback;
            Context_Feedback context_feedback = new Context_Feedback();
            Device SR = selected_SR.Single().Key;
            double trust = selected_SR.Single().Value;

            if (SR.SR_IsTaskDone == true)
            {
                if (SP.Services.Time_Using.Equals(SR.Services.Time_Using))
                    T1 = 0; //True
                else
                    T1 = 1; //False

                Delta_Time = Math.Abs(Service.GetNumberByTimeUsing(SR.Services.Time_Using) - Service.GetNumberByTimeUsing(SP.Services.Time_Using));

                ///////
                if (SP.Services.OoS.Equals(SR.Services.OoS))
                    T2 = 0;//true
                else
                    T2 = 1;//False

                Delta_Resource = Math.Abs(Service.GetNumberByReosurcesUsing(SR.Services.Reosurces_Using) - Service.GetNumberByReosurcesUsing(SP.Services.Reosurces_Using));

                if (SP.ground_Trust > 0.55 && SP.ground_Trust < 0.60)
                    T3 = 1;//Bad//Penalty
                else
                    T3 = 0;//Good//Reward

               
                ////2-Good
                A = SR.ground_Trust;
                B = 1 - SR.ground_Trust;
                feedback = ((double)A / (A + B));

                context_feedback.Id = SR.Id;
                context_feedback.Feedback = feedback;
                context_feedback.Location_Service = Unit.GetNumberByUnit(SR.Current_Location);
                context_feedback.Time_Service = Service.GetNumberByTime(SR.Services.Time_Service);
                context_feedback.Services_Requetsed = Service.GetNumberByService(SR.Services.Services_Requetsed);

                if (SP.Visited_SR_Feedback.Count() == 0 || (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Id)) && SP.Visited_SR_Feedback.Single(p => p.Key.Id.Equals(SR.Id)).Value.Count == 0))
                {
                    SP.Visited_SR_Feedback.Add(SR, new List<Context_Feedback> { context_feedback });

                }
                else if (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Id)) && SP.Visited_SR_Feedback.Single(p => p.Key.Id.Equals(SR.Id)).Value.Count == 0)
                {
                    SP.Visited_SR_Feedback.Single(p => p.Key.Id.Equals(SR.Id)).Value.Add(context_feedback);
                }
            }
        }

        /// <summary>
        /// Performs post-evaluation for a service provider after transactions with all trustworthy service requesters, updating feedback.
        /// </summary>
        public void SP_PostEvaluation(Device SP)
        {
            //Feedback Value
            Random r = new Random();
            Dictionary<Device, double[]> dictionary_Feedback = new Dictionary<Device, double[]>();
            double positive = 0, negative = 0;
            int T, T1, T2, T3 = 0;
            int time_Real, time_Expected, service_Real, service_Expected, link_Before, link_After;
            double Delta, Delta_Time, Delta_Resource, Delta_Link;
            //double A = 1, B = 1, a = 0.02, b = 0.02, x = 2;
            //double A = 1, B = 1, a = 0.9, b = 0.1, x = 2;
            double A = 1, B = 1, a = 0.4, b = 0.6, x = 2;
            double feedback;
            Context_Feedback context_feedback = new Context_Feedback();
            //Device SR = selected_SR.Single().Key;
            //double trust = selected_SR.Single().Value;

            //if (SR.SR_IsTaskDone == true)
            //{
            foreach (var SR in SP.Trusthworthiness_SR)
            {
                if (SP.Services.Time_Using.Equals(SR.Key.Services.Time_Using))
                    T1 = 0; //True
                else
                    T1 = 1; //False

                Delta_Time = Math.Abs(Service.GetNumberByTimeUsing(SR.Key.Services.Time_Using) - Service.GetNumberByTimeUsing(SP.Services.Time_Using));

                ///////
                if (SP.Services.OoS.Equals(SR.Key.Services.OoS))
                    T2 = 0;//true
                else
                    T2 = 1;//False

                Delta_Resource = Math.Abs(Service.GetNumberByReosurcesUsing(SR.Key.Services.Reosurces_Using) - Service.GetNumberByReosurcesUsing(SP.Services.Reosurces_Using));

                if (SR.Key.ground_Trust > 0.55 && SR.Key.ground_Trust < 0.60)
                    T3 = 1;//Bad//Penalty
                else
                    T3 = 0;//Good//Reward

                
                Delta = 1 - Math.Abs(SR.Key.ground_Trust - SR.Value);

                //feedback = 1 - Math.Abs(SR.Key.ground_Trust - SR.Value );
                //feedback = (double) (A / (A + B) * trust);
                double trust = SR.Value;

                A = SP.ground_Trust;
                B = 1 - SP.ground_Trust;
                //feedback = ((double)A / (A + B));
                //feedback = ((double)A / (A + B)) * trust;
                feedback = A * trust;

                context_feedback.Id = SR.Key.Id;
                context_feedback.Feedback = feedback;
                context_feedback.Location_Service = Unit.GetNumberByUnit(SR.Key.Current_Location);
                context_feedback.Time_Service = Service.GetNumberByTime(SR.Key.Services.Time_Service);
                context_feedback.Services_Requetsed = Service.GetNumberByService(SR.Key.Services.Services_Requetsed);

                if (SP.Visited_SR_Feedback.Count() == 0 || (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Key.Id)) && SP.Visited_SR_Feedback.Single(p => p.Key.Id.Equals(SR.Key.Id)).Value.Count == 0))
                {
                    SP.Visited_SR_Feedback.Add(SR.Key, new List<Context_Feedback> { context_feedback });
                }
                else if (SP.Visited_SR_Feedback.Any(p => p.Key.Id.Equals(SR.Key.Id)) && SP.Visited_SR_Feedback.Single(p => p.Key.Id.Equals(SR.Key.Id)).Value.Count == 0)
                {
                    SP.Visited_SR_Feedback.Single(p => p.Key.Equals(SR.Key.Id)).Value.Add(context_feedback);
                }
            }
        }

        //*********************************************************END************************************************************//
    }
}
