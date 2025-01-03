using Steamworks;
using System.Diagnostics;
using System.Timers;

namespace ConsoleApp2
{
    internal class Program
    {
        protected static bool m_bInitialized = false;
        protected static SteamAPIWarningMessageHook_t? m_SteamAPIWarningMessageHook =null;
        public static System.Timers.Timer timer = null;

        private static CGameID m_GameID;
        protected static CallResult<CreateItemResult_t> m_CreateItemResult;
        protected static CallResult<SubmitItemUpdateResult_t> m_SubmitItemResult;
        protected static CallResult<SteamUGCQueryCompleted_t> m_QueryItemResult;

        public static string title = "";
        public static string description = "";
        public static string preview= "";
        public static string content="";
        public static string changeNote="";
        protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Console.WriteLine(pchDebugText);
        }
        static void EndProgram()
        {
            Console.ReadLine();
            SteamAPI.Shutdown();//end
        }
        static void Main(string[] args)
        {
            Console.WriteLine("此程序必填：title.txt content.txt，等会报错具体说怎么填");
            if (!Packsize.Test())
            {
                Console.WriteLine("Steamworks.NET.dll有问题，建议：发群问print_0()   [Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }
            if (!DllCheck.Test())
            {
                Console.WriteLine("steam_api64.dll/steam_api.dll有问题，建议：发群问print_0   [Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }
            try
            {
                if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
                {
                    Console.WriteLine("steam让你重新运行这个程序（），建议：发群问print_0   [Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");
                    EndProgram();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            {
                Console.WriteLine("没有steam_api64.dll/steam_api.dll，建议：下载   [Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                EndProgram();
                return;
            }
            m_bInitialized = SteamAPI.Init();
            if (!m_bInitialized)
            {
                Console.WriteLine("steam没启动？steam_appid.txt删了？，建议：实在不行chong   [Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
                EndProgram();
                return;
            }


            if (m_SteamAPIWarningMessageHook == null)
            {
                m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
            }

            string name = SteamFriends.GetPersonaName();
            Console.WriteLine(name);


            m_GameID = new CGameID(SteamUtils.GetAppID());
            m_CreateItemResult = CallResult<CreateItemResult_t>.Create(new CallResult<CreateItemResult_t>.APIDispatchDelegate(CreateItemResult));
            m_SubmitItemResult = CallResult<SubmitItemUpdateResult_t>.Create(new CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate(SubmitUpdateResult));
            m_QueryItemResult = CallResult<SteamUGCQueryCompleted_t>.Create(new CallResult<SteamUGCQueryCompleted_t>.APIDispatchDelegate(QueryItemResult));
            
            if (!File.Exists("title.txt"))
            {
                Console.WriteLine("我“title.txt”呢？（已自动生成）   title.txt is not exist");
                File.Create("title.txt");
                EndProgram();
                return ;
            }
            title=File.ReadAllText("title.txt");
            if (title.Length == 0)
            {
                Console.WriteLine("填一下“title.txt”就是你上传的mod名字   ERROR_ title.txt is empty");
                EndProgram();
                return;
            }
            if (!File.Exists("content.txt"))
            {
                Console.WriteLine("我“content.txt”呢？（已自动生成）   content.txt is not exist");
                File.Create("content.txt");
                EndProgram();
                return;
            }
            content = File.ReadAllText("content.txt");
            if (content.Length == 0) {
                Console.WriteLine("“content.txt”没填内容，填一个你要上传的文件夹绝对路径，比如说：E:\\BaiduNetdiskDownload（支撑中文路径）   ERROR_ content.txt is empty");
                EndProgram();
                return;
            }
            if (!Directory.Exists(content)&&!File.Exists(content))
            {
                Console.WriteLine("你填的上传的文件夹路径不存在   ERROR_ content folder/file is not exist");
                EndProgram();
                return;
            }
            if (File.Exists("description.txt"))
            {
                description = File.ReadAllText("description.txt");
            }
            else { File.Create("description.txt"); }
            if (File.Exists("preview.txt"))
            {
                preview = File.ReadAllText("preview.txt");
                if (preview.Length!=0&&!File.Exists(preview)) {
                    Console.WriteLine("你填的封面路径不存在，比如填E:\\image\\a.png   preview img is not exist");
                    EndProgram();
                    return ;
                }
            }
            else { File.Create("preview.txt"); }
            if (File.Exists("changeNote.txt"))
            {
                changeNote = File.ReadAllText("changeNote.txt");
            }
            else { File.Create("changeNote.txt"); }

            
            
            UGCQueryHandle_t uGCQueryHandle_T = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByPublicationDate, EUGCMatchingUGCType.k_EUGCMatchingUGCType_All,
                SteamUtils.GetAppID(),
                SteamUtils.GetAppID(),
                1);/*SteamUGC.CreateQueryUserUGCRequest(
                SteamUser.GetSteamID().GetAccountID(),
                EUserUGCList.k_,
                EUGCMatchingUGCType.k_EUGCMatchingUGCType_All,
                EUserUGCListSortOrder.k_EUserUGCListSortOrder_SubscriptionDateDesc,
                SteamUtils.GetAppID(),
                SteamUtils.GetAppID(),
                1
            );//Community代表可以下载的创意工坊物品*/
            SteamAPICall_t steamAPICall_T = SteamUGC.SendQueryUGCRequest(uGCQueryHandle_T);
            m_QueryItemResult.Set(steamAPICall_T);

            //Console.WriteLine("UGC QueryItem!");
            timer = new System.Timers.Timer();
            timer.Interval = 10;
            timer.AutoReset = false;
            timer.Enabled = false;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            EndProgram();
        }
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            SteamAPI.RunCallbacks();
            timer.Enabled = true;//为true后，会再次调用Timer_Elapsed事件
                                 //Console.WriteLine("---------------------定时器开始---------------------");
                                 //for (int i = 1; i <= 7; i++)
                                 //{
                                 //    System.Threading.Thread.Sleep(1000);
                                 //    Console.WriteLine("i:{0}", i);
                                 //}
                                 //Console.WriteLine("---------------------定时器结束---------------------");
        }
        private static void CreateItemResult(CreateItemResult_t callback, bool bIOFailure)
        {
            switch (callback.m_eResult)
            {
                case EResult.k_EResultTimeout:
                    Console.WriteLine("上传超时，整个加速器？  Error Timeout: Current user is not currently logged into steam");
                    break;
                case EResult.k_EResultNotLoggedOn:
                    Console.WriteLine("没登录？  TError Not logged on: The user creating the item is currently banned in the community");
                    break;
                case EResult.k_EResultInsufficientPrivilege:
                    Console.WriteLine("截图发群，可以问下print_0  Error Insufficient Privilege: The user creating the item is currently banned in the community");
                    break;
                case EResult.k_EResultOK:
                    ulong publishedFileId = callback.m_nPublishedFileId.m_PublishedFileId;
                    //this.showLoadingSpinner = false;
                    //this.ActiveInfo.WorkshopPublishID = string.Concat((object)callback.m_nPublishedFileId.m_PublishedFileId);
                    Console.WriteLine("已创建物品，上传内容ing（别关！）   CreateItem Result Ok!\n");
                    if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
                    {
                        SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + (object)callback.m_nPublishedFileId);
                    }
                    PublishedFileId_t m_nPublishedFileId = callback.m_nPublishedFileId;
                    UpdateItem(m_nPublishedFileId);
                    break;
            }
        }
        private static void UpdateItem(PublishedFileId_t m_nPublishedFileId)
        {
            UGCUpdateHandle_t m_updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), m_nPublishedFileId);
            if (!SteamUGC.SetItemTitle(m_updateHandle, title))
            {
                Console.WriteLine("标题设置失败  Set Tile Fail");
            }
            if (!SteamUGC.SetItemContent(m_updateHandle, content))
            {
                Console.WriteLine("内容设置失败  Set Item Content Fail");
            }
            if (!SteamUGC.SetItemPreview(m_updateHandle, preview))//C:/Users/print/source/repos/ConsoleApp2/bin/Debug/net8.0/a.png
            {
                Console.WriteLine("封面设置失败   Set Item Preview Fail");
            }
            if (!SteamUGC.SetItemDescription(m_updateHandle, description))
            {
                Console.WriteLine("描述设置失败   Set Item Description Fail");
            }
            if (!SteamUGC.SetItemVisibility(m_updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic))
            {
                Console.WriteLine("可见性设置失败   Set Item Visibility Fail");
            }
            SteamAPICall_t steamAPICall_T = SteamUGC.SubmitItemUpdate(m_updateHandle, changeNote);
            m_SubmitItemResult.Set(steamAPICall_T);
        }

        private static void SubmitUpdateResult(SubmitItemUpdateResult_t callback, bool bIOFailure)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                Console.WriteLine("okok上传成功，可以回车关了    Upload completed successfully");
            }
            else
            {
                Console.WriteLine("上传失败，错误代码：  "+ (object)callback.m_eResult + "    Update Failed, Error code: " + (object)callback.m_eResult);
            }
        }

        private static void QueryItemResult(SteamUGCQueryCompleted_t callback, bool bIOFailure)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                //Console.WriteLine("Query completed successfully");
                for (uint index = 0; index < callback.m_unTotalMatchingResults; index++)
                {
                    SteamUGCDetails_t details;
                    if (SteamUGC.GetQueryUGCResult(callback.m_handle, index, out details))
                    {
                        if (details.m_rgchTitle == title)
                        {
                            Console.WriteLine("UGC item: " + index + " " + details.m_nPublishedFileId + "   " + details.m_rgchTitle);
                            Console.WriteLine("开始更新（别关）  UGC CreateItem!");
                            UpdateItem(details.m_nPublishedFileId);
                            return;
                        }
                    }
                }
                SteamAPICall_t steamAPICall_T = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);//Community代表可以下载的创意工坊物品
                m_CreateItemResult.Set(steamAPICall_T);
                Console.WriteLine("开始上传（别关）  UGC CreateItem!");
            }
            else
            {
                Console.WriteLine("Query Failed, Error code: " + (object)callback.m_eResult);
            }
            SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
        }
    }
}
