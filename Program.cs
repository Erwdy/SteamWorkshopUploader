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
            if (!Packsize.Test())
            {
                Console.WriteLine("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }
            if (!DllCheck.Test())
            {
                Console.WriteLine("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }
            try
            {
                if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
                {
                    Console.WriteLine("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");
                    EndProgram();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            {
                Console.WriteLine("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                EndProgram();
                return;
            }
            m_bInitialized = SteamAPI.Init();
            if (!m_bInitialized)
            {
                Console.WriteLine("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
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
                Console.WriteLine("title.txt is not exist");
                EndProgram();
                return ;
            }
            title=File.ReadAllText("title.txt");
            if (title.Length == 0)
            {
                Console.WriteLine("ERROR_ title.txt is empty");
                EndProgram();
                return;
            }
            if (!File.Exists("content.txt"))
            {
                Console.WriteLine("content.txt is not exist");
                EndProgram();
                return;
            }
            content = File.ReadAllText("content.txt");
            if (content.Length == 0) {
                Console.WriteLine("ERROR_ content.txt is empty");
                EndProgram();
                return;
            }
            if (!Directory.Exists(content)&&!File.Exists(content))
            {
                Console.WriteLine("ERROR_ content folder/file is not exist");
                EndProgram();
                return;
            }
            if (File.Exists("description.txt"))
            {
                description = File.ReadAllText("description.txt");
            }
            if (File.Exists("preview.txt"))
            {
                preview = File.ReadAllText("preview.txt");
                if (preview.Length!=0&&!File.Exists(preview)) {
                    Console.WriteLine("preview img is not exist");
                    EndProgram();
                    return ;
                }
            }
            if (File.Exists("changeNote.txt"))
            {
                changeNote = File.ReadAllText("changeNote.txt");
            }
            SteamAPICall_t steamAPICall_T = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);//Community代表可以下载的创意工坊物品
            m_CreateItemResult.Set(steamAPICall_T);
            Console.WriteLine("UGC CreateItem!");

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
                    Console.WriteLine("Error Timeout: Current user is not currently logged into steam");
                    break;
                case EResult.k_EResultNotLoggedOn:
                    Console.WriteLine("TError Not logged on: he user creating the item is currently banned in the community");
                    break;
                case EResult.k_EResultInsufficientPrivilege:
                    Console.WriteLine("Error Insufficient Privilege: The user creating the item is currently banned in the community");
                    break;
                case EResult.k_EResultOK:
                    ulong publishedFileId = callback.m_nPublishedFileId.m_PublishedFileId;
                    //this.showLoadingSpinner = false;
                    //this.ActiveInfo.WorkshopPublishID = string.Concat((object)callback.m_nPublishedFileId.m_PublishedFileId);
                    Console.WriteLine("CreateItem Result Ok!\n");
                    if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
                    {
                        SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + (object)callback.m_nPublishedFileId);
                    }
                    PublishedFileId_t m_nPublishedFileId = callback.m_nPublishedFileId;
                    UGCUpdateHandle_t m_updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), m_nPublishedFileId);
                    if (!SteamUGC.SetItemTitle(m_updateHandle, title))
                    {
                        Console.WriteLine("Set Tile Fail");
                    }
                    if (!SteamUGC.SetItemContent(m_updateHandle, content))
                    {
                        Console.WriteLine("Set Item Content Fail");
                    }
                    if (!SteamUGC.SetItemPreview(m_updateHandle, preview))//C:/Users/print/source/repos/ConsoleApp2/bin/Debug/net8.0/a.png
                    {
                        Console.WriteLine("Set Item Preview Fail");
                    }
                    if (!SteamUGC.SetItemDescription(m_updateHandle, description))
                    {
                        Console.WriteLine("Set Item Description Fail");
                    }
                    if (!SteamUGC.SetItemVisibility(m_updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic))
                    {
                        Console.WriteLine("Set Item Visibility Fail");
                    }
                    SteamAPICall_t steamAPICall_T = SteamUGC.SubmitItemUpdate(m_updateHandle, changeNote);
                    m_SubmitItemResult.Set(steamAPICall_T);
                    break;
            }
        }

        private static void SubmitUpdateResult(SubmitItemUpdateResult_t callback, bool bIOFailure)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                Console.WriteLine("Upload completed successfully");
            }
            else
            {
                Console.WriteLine("Update Failed, Error code: " + (object)callback.m_eResult);
            }
        }

        private static void QueryItemResult(SteamUGCQueryCompleted_t callback, bool bIOFailure)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                Console.WriteLine("Query completed successfully");
                for (uint index = 0; index < callback.m_unTotalMatchingResults; index++)
                {
                    SteamUGCDetails_t details;

                    if (SteamUGC.GetQueryUGCResult(callback.m_handle, index, out details))
                    {
                        Console.WriteLine("UGC item: " + index + " " + details.m_nPublishedFileId);
                    }
                }
            }
            else
            {
                Console.WriteLine("Query Failed, Error code: " + (object)callback.m_eResult);
            }
            SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
        }
    }
}
