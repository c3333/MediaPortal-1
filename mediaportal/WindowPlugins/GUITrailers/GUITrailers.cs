/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class GUITrailers : GUIWindow
	{
		[SkinControlAttribute(3)]			protected GUISelectButtonControl btnletter=null;
		[SkinControlAttribute(4)]			protected GUIListControl listview=null;
		[SkinControlAttribute(5)]			protected GUIToggleButtonControl btntoggleplot = null;
		[SkinControlAttribute(6)]			protected GUIToggleButtonControl btntogglecast = null;
		[SkinControlAttribute(24)]			protected GUIImage poster =null;
		[SkinControlAttribute(50)]			protected GUILabelControl label0 = null;
		[SkinControlAttribute(51)]			protected GUIFadeLabel label1 = null;
		[SkinControlAttribute(52)]			protected GUILabelControl label2 = null; //runtime
		[SkinControlAttribute(53)]			protected GUILabelControl label3 = null;
		[SkinControlAttribute(54)]			protected GUILabelControl label4 = null;
		[SkinControlAttribute(55)]			protected GUILabelControl label5 = null;
		[SkinControlAttribute(56)]			protected GUILabelControl label6 = null;
		[SkinControlAttribute(57)]			protected GUITextScrollUpControl castarea=null;
		[SkinControlAttribute(58)]			protected GUILabelControl label8 =null;
		[SkinControlAttribute(59)]			protected GUITextScrollUpControl plotarea=null;
		//[SkinControlAttribute(60)]			protected GUIImageList labelrating =null;

		#region Variables
        
		int Main_Selected = 0; // remember mainvielist selected
		int Movie_Selected = 0; // remember movie selected in listview
		int TCM_Selected = 0; // remberer selections Trailers, Clips, More View
		int Prev_SelectedItem = 0;	// remember listview selections

		string TempHTML;	// GetWeb string
		string MMSUrl;
		string currentletter ="";
		string casturl;
		string backgroundposter = null;

		DateTime RefreshDaily = DateTime.Now.Date; //for the daily update, if HTPC is on 24h it will refresh strings every day.
		
		string[] MovieURL = new string[2000];	// strings for all movies
		string[] MovieName = new string[2000];

		string[] LMovieUrl = new string[200]; // strings for letterbutton movies
		string[] LMovieName = new string[200];

		string[] JAMovieUrl = new string[50]; // strings for JustAdded movies
		string[] JAMovieName = new string[50];

		string[] MWMovieUrl = new string[50]; //  strings for MostWatched movies
		string[] MWMovieName = new string[50];

		string[] Trailers = new string[25];
		string[] TrailersUrl = new string[25];

		string[] Clips = new string[25];
		string[] ClipsUrl = new string[25];

		string[] More = new string[25];
		string[] MoreUrl = new string[25];

		bool foundt = false;		// bools for if trailer, clips or more is found
		bool foundc = false;
		bool foundm = false;

		bool mainview = false;
		bool allview = false; // bools for reminding which view the user is in
		bool tcmview = false;
		bool tview = false;
		bool cview = false;
		bool mview = false;
		bool jaview = false;
		bool mwview = false;
		bool letterview = false;
		bool plotview = true;
		bool castview = false;

		
		string ptitle;			// Used by SetGUIProperties, GetGUIProperties
		string pgenre;			// Before MP start playing a movie fullscreen
		string pruntime;		// it wil save the #tags info in these string
		string preleasedate;    // and will load them back after playing the 
		string pplot;			// movie.
		string pcast;
		double prating;

		// Get from mediaportal.xml
		string bitrate = string.Empty;
		bool Show_GT = false;

		// BackGroundworker
		string _downloadedText		= string.Empty;

		// For Downloading trailers
//		string DownloadFileName = string.Empty;
//		string DownloadFileUrl = string.Empty;
//		bool downloadfileview = false;

                
		#endregion

		public override int GetID
		{
			get
			{
				return 5900;
			}
			set
			{
			}
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin+@"\mytrailers.xml");
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)  // For when a button is pressed
		{
			if (control==btnletter)
				OnButtonTwo();
			if (control==listview)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
				OnMessage(msg);
				int itemIndex = (int)msg.Param1;
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(itemIndex);
				}
			}
			if (control==btntoggleplot)
				ToggleButtonPlot();
			if(control==btntogglecast)
				ToggleButtonCast();
			base.OnClicked (controlId, control, actionType);
		}
		public override void OnAction(Action action)
		{
			if(action.wID == Action.ActionType.ACTION_STOP)
				SetGUIProperties();
			base.OnAction (action);
		}

		protected override void OnPageDestroy(int newWindowId)
		{
			if(G_viewInfoAndTrailer==true || tview==true || cview==true || mview==true)
			{
				Prev_SelectedItem = listview.SelectedListItemIndex;
			}
			GetGUIProperties();
			base.OnPageDestroy (newWindowId);
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			btnletter.RestoreSelection = false;
			btnletter.AddSubItem("#");
			for(char k = 'A'; k <= 'Z'; k++)
			{
				btnletter.AddSubItem(k.ToString());
			}
			LoadSettings();
			if(G_viewInfoAndTrailer==true || tview==true || cview==true || mview==true)
			{
				listview.SelectedListItemIndex = Prev_SelectedItem;
				listview.Focus = true;
			}
			else
			{
				ShowLabelsFalse();
				GUIPropertyManager.SetProperty("#title", "");
				if(backgroundposter==null)
                    backgroundposter = poster.FileName;
				poster.SetFileName(GUIGraphicsContext.Skin+@"\media\"+backgroundposter);
				ShowMainListView();
				listview.Focus=true;
			}
		}

		protected override void OnPreviousWindow()
		{
			base.OnPreviousWindow ();
		}

	
        // Tryed to create to download file with nice notify window, works for http urls but not
		// for mms:// urls so this can be used for apple movies maybe?
//		protected override void OnShowContextMenu()
//		{
//			base.OnShowContextMenu ();
//			GUIListItem item=listview.SelectedListItem;
//			int itemNo=listview.SelectedListItemIndex;
//			if (item==null) return;
//			if (downloadfileview==false) return;
//
//			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
//			if (dlg==null) return;
//			dlg.Reset();
//			dlg.SetHeading(924); // menu
//			dlg.Add("Download");
//			dlg.Add("Download & Play");
//			dlg.DoModal(GetID);
//			if(dlg.SelectedLabelText.Equals("Download"))
//			{
//				GetMMSURL(TrailersUrl[itemNo]);
//				DownloadFile();
//			}
//		
//		}
		
		private void OnButtonTwo()
		{
			if(RefreshDaily.Equals(DateTime.Now.Date)==false) // daily update
				Array.Clear(MovieName,0,2000);
			if(MovieName[0]==null)
				GetTrailers();

			int i = 0;
			int j = 0;
			string letter;
			listview.Clear();
			Array.Clear(LMovieName,0,200);
			Array.Clear(LMovieUrl,0,200);
			ResetViews();
			letterview=true;
			allview=false;
			jaview=false;
			mwview=false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\"+backgroundposter);
			GUIPropertyManager.SetProperty("#title","");
			letter = btnletter.SelectedLabel;
		
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);
						
			while(MovieName[i]!=null)
			{
				if(MovieName[i].StartsWith("The "+letter)==true)
				{
					LMovieName[j] = MovieName[i];
					LMovieUrl[j] = MovieURL[i];
					GUIListItem item = new GUIListItem();
					item.Label = LMovieName[j];
					item.IsFolder = true;
					Utils.SetDefaultIcons(item);
					listview.Add(item);
					j++;
				}
				else if(MovieName[i].StartsWith("The ")==true) {}
				else if(MovieName[i].StartsWith("A "+letter)==true)
				{
					LMovieName[j] = MovieName[i];
					LMovieUrl[j] = MovieURL[i];
					GUIListItem item = new GUIListItem();
					item.Label = LMovieName[j];
					item.IsFolder = true;
					Utils.SetDefaultIcons(item);
					listview.Add(item);
					j++;
				}
				else if(MovieName[i].StartsWith("A ")==true) {}
				else if(MovieName[i].StartsWith("An "+letter)==true)
				{
					LMovieName[j] = MovieName[i];
					LMovieUrl[j] = MovieURL[i];
					GUIListItem item = new GUIListItem();
					item.Label = LMovieName[j];
					item.IsFolder = true;
					Utils.SetDefaultIcons(item);
					listview.Add(item);
					j++;
				}
				else if(MovieName[i].StartsWith("An ")==true) {}
				else if(MovieName[i].StartsWith(letter)==true)
				{
					LMovieName[j] = MovieName[i];
					LMovieUrl[j] = MovieURL[i];
					GUIListItem item = new GUIListItem();
					item.Label = LMovieName[j];
					listview.Add(item);
					item.IsFolder = true;
					Utils.SetDefaultIcons(item);
					j++;
				}
				else if(letter.Equals("#")==true)
				{
					for(int n = 0; n <= 9; n++)
						if(MovieName[i].StartsWith(n.ToString())==true)
						{
							LMovieName[j] = MovieName[i];
							LMovieUrl[j] = MovieURL[i];
							GUIListItem item = new GUIListItem();
							item.Label = LMovieName[j];
							item.IsFolder = true;
							Utils.SetDefaultIcons(item);
							listview.Add(item);
							j++;
						}
				}

				i++;
			}
			if(currentletter==letter)
				listview.SelectedListItemIndex = Prev_SelectedItem;
			else
			{
				listview.SelectedListItemIndex = 0;	
				currentletter = letter;
			}
		}
		private void ToggleButtonPlot()
		{
			btntoggleplot.Selected=true;
			btntogglecast.Selected=false;
			plotarea.Visible=true;
			castarea.Visible=false;
			label8.Visible=true;
			label6.Visible=false;
		}
		private void ToggleButtonCast()
		{
			if(G_viewInfoAndTrailer==true)
				GUIPropertyManager.SetProperty("cast", G_Cast[GermanSelected]);
			else GetCastInfo(casturl);
			btntoggleplot.Selected=false;
			btntogglecast.Selected=true;
			plotarea.Visible=false;
			castarea.Visible=true;
			label8.Visible=false;
			label6.Visible=true;
		}

		private void OnClick(int itemindex) // // When something is pressed in the listview
		{
			// Trailer, Clips, Movie listview
			if(tcmview==true)
			{
				if(itemindex==0)
				{
					TCM_Selected = 0;
					if(allview==true)
						ShowMovies();
					else if(jaview==true)
						ShowJustAdded();
					else if(mwview==true)
						ShowMostWatched();
					else if(letterview==true)
						OnButtonTwo();
				}
				else if(itemindex==1)
				{
					TCM_Selected = 1;
					if(foundt==true)
						ShowTrailers();
					else if(foundt==false & foundc==true)
						ShowClips();
				}
				else if(itemindex==2)
				{
					TCM_Selected = 2;
					if(foundt==false & foundc==true)
						ShowMore();
					else if(foundc==true)
						ShowClips();
				}
				else if(itemindex==3)
				{
					TCM_Selected = 3;
					ShowMore();
				}
			}
				// Trailerview
			else if(tview==true)
			{
				if(itemindex==0)
					ShowTrailersClipsMore();
				else
				{
					Prev_SelectedItem = listview.SelectedListItemIndex;
					Play(TrailersUrl[itemindex-1]);
				}
			}
				// Clipsview
			else if(cview==true)
			{
				if(itemindex==0)
					ShowTrailersClipsMore();
				else
				{
					Prev_SelectedItem = listview.SelectedListItemIndex;
					Play(ClipsUrl[itemindex-1]);
				}

			}
				// Moreview
			else if(mview==true)
			{
				if(itemindex==0)
					ShowTrailersClipsMore();
				else 
				{
					Prev_SelectedItem = listview.SelectedListItemIndex;
					Play(MoreUrl[itemindex-1]);
				}
			}
				// JustAddedview
			else if(jaview==true)
			{
				if(itemindex==0)
				{
					Movie_Selected = 0;
					ShowMainListView();
				}
				else
				{
					itemindex = itemindex-1;
					Movie_Selected = listview.SelectedListItemIndex;
					GetMovieInfo(JAMovieUrl[itemindex], JAMovieName[itemindex], itemindex);
					ShowTrailersClipsMore();
					ShowMovieInfo(JAMovieUrl[itemindex], JAMovieName[itemindex], itemindex);
				}
			}
				// MostWatchedview
			else if(mwview==true)
			{
				if(itemindex==0)
				{
					Movie_Selected = 0;
					ShowMainListView();
				}
				else
				{
					itemindex = itemindex-1;
					Movie_Selected = listview.SelectedListItemIndex;
					GetMovieInfo(MWMovieUrl[itemindex], MWMovieName[itemindex], itemindex);
					ShowTrailersClipsMore();
					ShowMovieInfo(MWMovieUrl[itemindex], MWMovieName[itemindex], itemindex);
				}
			}
				// Letterbutton view
			else if(letterview==true)
			{
				if(itemindex==0)
				{
					Movie_Selected = 0;
					ShowMainListView();
				}
				else
				{
					GetMovieInfo(LMovieUrl[itemindex-1], LMovieName[itemindex-1], itemindex);
					ShowTrailersClipsMore();
					ShowMovieInfo(LMovieUrl[itemindex-1], LMovieName[itemindex-1], itemindex);
				}
			}
				// All movies view
			else if(allview==true)
			{
				if(itemindex==0)
				{
					Movie_Selected = 0;
					ShowMainListView();
				}
				else
				{
					itemindex = itemindex-1;
					Movie_Selected = listview.SelectedListItemIndex;
					GetMovieInfo(MovieURL[itemindex], MovieName[itemindex], itemindex);
					ShowTrailersClipsMore();
					ShowMovieInfo(MovieURL[itemindex], MovieName[itemindex], itemindex);
				}
			}
				// Main selection view
			else if(mainview==true)
			{
				if(itemindex==0) // just added movies
				{
					Main_Selected = listview.SelectedListItemIndex;
					if(RefreshDaily.Equals(DateTime.Now.Date)==false) // For the daily refresh
						Array.Clear(JAMovieName,0,50);
					if(JAMovieName[0]==null)
					{GetJustAdded();}
					ShowJustAdded();
				}
				if(itemindex==1) // mostwatched movies
				{
					Main_Selected = listview.SelectedListItemIndex;
					if(RefreshDaily.Equals(DateTime.Now.Date)==false)
						Array.Clear(MWMovieName,0,50);
					if(MWMovieName[0]==null)
					{GetMostWatched();}
					ShowMostWatched();
				}
				if(itemindex==2) // all movies
				{
					Main_Selected = listview.SelectedListItemIndex;
					if(RefreshDaily.Equals(DateTime.Now.Date)==false)
						Array.Clear(MovieName,0,2000);
					if(MovieName[0]==null)
					{GetTrailers();}
					ShowMovies();
				}
				if(itemindex==3) // german trailers
				{
					Main_Selected = listview.SelectedListItemIndex;
					ShowGermanLayoutWoche();
				}
			}
				// German Trailerview Woche
			else if(G_viewWoche==true)
			{
				if(itemindex==0)
					ShowMainListView();
				if(itemindex==1)
				{
					GetGermanTrailers("http://de.movies.yahoo.com/mvsl.html");
					ShowGermanTrailers();
				}
				if(itemindex==2)
				{
					GetGermanTrailers("http://de.movies.yahoo.com/neu_im_kino.html");
					ShowGermanTrailers();
				}
				if(itemindex==3)
				{
					GetGermanTrailers("http://de.movies.yahoo.com/mvsn.html");
					ShowGermanTrailers();
				}
			}
				// German TrailerMovies view
			else if(G_viewMovie==true)
			{
				if(itemindex==0)
				{
					Movie_Selected = 0;
					ShowGermanLayoutWoche();
				}
				else
				{
					Movie_Selected = listview.SelectedListItemIndex;
					ShowGermanMovieInfo(GermanMovieName[itemindex-1],itemindex-1);
				}
			}
				// German movie info and single trailer view
			else if(G_viewInfoAndTrailer==true)
			{
				if(itemindex==0)
					ShowGermanTrailers();
				if(itemindex==1)
				{
					if(GermanTrailerURL[GermanSelected]!=null)
						Prev_SelectedItem = listview.SelectedListItemIndex;
					PlayGermanTrailer(GermanTrailerURL[GermanSelected]);
				}
			}
		}


		void GetWebPage(string url, out string HTMLDownload) // Get url and put in string
		{
			if(_workerCompleted)
			{
				_workerCompleted = false;

				BackgroundWorker worker = new BackgroundWorker();

				worker.DoWork += new DoWorkEventHandler(DownloadWorker);
				worker.RunWorkerAsync(url);

				using(WaitCursor cursor = new WaitCursor())
				{
					while(_workerCompleted == false)
						GUIWindowManager.Process();
				}

				HTMLDownload = _downloadedText;

				_downloadedText = null;
			}
			else
			{
				HTMLDownload = string.Empty;
			}
		}

		void DownloadWorker(object sender, DoWorkEventArgs e)
		{
			WebClient wc = new WebClient();

			try
			{
				byte[] HTMLBuffer;

				HTMLBuffer = wc.DownloadData((string)e.Argument);
				
				_downloadedText = Encoding.ASCII.GetString(HTMLBuffer);
			}
			catch(Exception ex)
			{
				Log.Write("GUITrailers.DownloadWorker: {0}", ex.Message);
			}
			finally
			{
				wc.Dispose();
			}

			_workerCompleted = true;
		}

		bool _workerCompleted = true;

		#region Download File in background and popup notify-window
//		void DownloadFile()
//		{
//			if(_workerDownloadFileCompleted)
//			{
//				_workerDownloadFileCompleted = false;
//
//				BackgroundWorker worker = new BackgroundWorker();
//
//				worker.DoWork +=new DoWorkEventHandler(DownloadFileWorker);
//				worker.RunWorkerAsync(DownloadFileUrl);
//
//				using (WaitCursor cursor = new WaitCursor())
//				{
//					while(_workerDownloadFileCompleted==false)
//						GUIWindowManager.Process();
//				}
//			}
//		}
//
//		void DownloadFileWorker(object sender, DoWorkEventArgs e)
//		{
////			string url = "http://movies.apple.com/movies/lionsgate/saw_2/saw_ii-tlr2_m480.mov";
////			string filename = @"c:\saw.mov";
//			
//			string url = MMSUrl;
//			string filename = @"c:\" +DownloadFileName + ".wmv";
//
//			WebClient wc = new WebClient();
//
//			try
//			{
//				wc.DownloadFile(url, filename);
//
//			}
//			catch(Exception ex)
//			{
//				Log.Write("Guitrailers.DownloadFile: {0}", ex.Message);
//			}
//			finally
//			{
//				wc.Dispose();
//			}
//
//			_workerDownloadFileCompleted = true;
//			
//			GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
//			dlgNotify.Reset();
//			dlgNotify.ClearAll();
//			dlgNotify.SetHeading("Download finished");
//			if(System.IO.File.Exists(@"thumbs\MPTemp -"+DownloadFileName + ".jpg")==true)
//                dlgNotify.SetImage(@"thumbs\MPTemp -"+DownloadFileName + ".jpg");
//			dlgNotify.SetText("finished download of trailer: "+ DownloadFileName);
//			dlgNotify.TimeOut = 5;
//			dlgNotify.DoModal(GetID);
//
//		}
//
//		bool _workerDownloadFileCompleted = true;
		#endregion

		void ShowMainListView()
		{
			ResetViews();
			mainview = true;
			allview = false;
			jaview = false;
			mwview = false;
			letterview = false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\"+backgroundposter);
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5902));

			string[] MainListOptions = new string[10];
			MainListOptions[0] = GUILocalizeStrings.Get(5903);
			MainListOptions[1] = GUILocalizeStrings.Get(5904);
			MainListOptions[2] = GUILocalizeStrings.Get(5905);
			MainListOptions[3] = GUILocalizeStrings.Get(5902);
			string language="";
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml")) 
			{
				language = xmlreader.GetValue("skin","language");
			}
			if(language.Equals("German")==true || Show_GT==true)
				MainListOptions[3] = GUILocalizeStrings.Get(5917);

			listview.Clear();
			int i = 0;
			while(MainListOptions[i] !=null)
			{
				GUIListItem item = new GUIListItem();
				item.IsFolder = true;
				Utils.SetDefaultIcons(item);
				item.Label = MainListOptions[i];
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Main_Selected;
		}

		void GetTrailers()
		{
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5910));
			GetWebPage(@"http://movies.yahoo.com/trailers/archive/", out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			MatchCollection mc = Regex.Matches(TempHTML, @"<a\shref=.(?<trailerurl>/shop.*).>(?<moviename>.*)</a>");

			int i = 0;

			foreach (Match m in mc)
			{
				MovieURL[i] = m.Groups["trailerurl"].Value;
				MovieName[i] = m.Groups["moviename"].Value;
				i++;
			}
			GUIPropertyManager.SetProperty("#title", "");
		}

		void ShowMovies()
		{
			ResetViews();
			allview = true;
			jaview = false;
			mwview = false;
			letterview = false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\"+backgroundposter);
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5905));
			
			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(MovieName[i] !=null)
			{
				GUIListItem item = new GUIListItem();
				item.IsFolder = true;
				Utils.SetDefaultIcons(item);
				item.Label = MovieName[i];
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Movie_Selected;
		}

		void GetMovieInfo(string url, string name, int i)
		{
			GUIPropertyManager.SetProperty("#title", "Getting moviesinfo...");
			ResetViews();
			tcmview = true;

			TempHTML = "";
			foundt = false;
			foundc = false;
			foundm = false;
			Array.Clear(Trailers,0,25);
			Array.Clear(TrailersUrl,0,25);
			Array.Clear(Clips,0,25);
			Array.Clear(ClipsUrl,0,25);
			Array.Clear(More,0,25);
			Array.Clear(MoreUrl,0,25);

			GetWebPage(@"http://movies.yahoo.com/"+url, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			MatchCollection mc = Regex.Matches(TempHTML, @"(?sx-m).*?(?:</tr>)");

			int t = 0;
			int c = 0;
			int mo = 0;

			foreach (Match m in mc)
			{
				// get trailers & teasers can be more then 1
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/trailers/")!=-1)
				{
					// search for 700 kbit stream
					if(bitrate.Equals("300")==false)
					{
						if(m.Value.IndexOf("700-p") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/trailers/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*).");
							TrailersUrl[t] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";

							Match m2 = Regex.Match(m.Value, @".(?<trailername>.*)</a>");
							Trailers[t] = m2.Groups["trailername"].Value;
							t++;
						}
					}
						// if there is no 700 kbit stream then get 300 kbit stream
					else
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/trailers/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*).");
						TrailersUrl[t] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"].Value + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Log.Write(m1.Groups["id"].Value);
						Log.Write(m1.Groups["segment"].Value);
						Log.Write(m1.Groups["movienumber"].Value);

						Match m2 = Regex.Match(m.Value, @".(?<trailername>.*)</a>");
						Trailers[t] = m2.Groups["trailername"].Value;
						Log.Write(m2.Groups["trailername"].Value);
						t++;
					}
					foundt = true;
				}
				// search for clips
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/clips/") !=-1)
				{
					if(bitrate.Equals("300")==false)
					{
						// search for 700 kbit stream
						if(m.Value.IndexOf("700-p") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							Clips[c] = m1.Groups["clipsname"].Value;

							c++;
						}
						else if(m.Value.IndexOf("700-s") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-700-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							Clips[c] = m1.Groups["clipsname"].Value;

							c++;
						}
					}

						// if there is no 700 kbit stream then get 300 kbit stream
					else if(m.Value.IndexOf("300-p") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Clips[c] = m1.Groups["clipsname"].Value;

						c++;
					}
					else if(m.Value.IndexOf("300-s") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-300-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Clips[c] = m1.Groups["clipsname"].Value;

						c++;
					}
					foundc = true;
				}
				// search for more "clips"
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/more/")!=-1)
				{
					if(bitrate.Equals("300")==false)
					{
						// search for 700 kbit stream
						if(m.Value.IndexOf("700-p") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							More[mo] = m1.Groups["clipsname"].Value;

							mo++;
						}
						else if(m.Value.IndexOf("700-s") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-700-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							More[mo] = m1.Groups["clipsname"].Value;

							mo++;
						}
					}
					// if there is no 700 kbit stream then get 300 kbit stream
					else if(m.Value.IndexOf("300-p") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						More[mo] = m1.Groups["clipsname"].Value;

						mo++;
					}
					else if(m.Value.IndexOf("300-s") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-300-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						More[mo] = m1.Groups["clipsname"].Value;

						mo++;
					}

					foundm = true;
				}
			}
			ptitle = name;
			GUIPropertyManager.SetProperty("#title", ptitle);
		}

		void ShowTrailersClipsMore()
		{
			ResetViews();
			tcmview=true;
			ShowLabelsTrue();
			SetGUIProperties();
			
			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);
			
			if(foundt==true)
			{
				GUIListItem item = new GUIListItem();
				item.Label = GUILocalizeStrings.Get(5906);
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				listview.Add(item);
			}
			if(foundc==true)
			{
				GUIListItem item = new GUIListItem();
				item.Label = GUILocalizeStrings.Get(5907);
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				listview.Add(item);
			}
			if(foundm==true)
			{
				GUIListItem item = new GUIListItem();
				item.Label = GUILocalizeStrings.Get(5908);
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				listview.Add(item);
			}
			listview.SelectedListItemIndex = TCM_Selected;
			listview.Focus = true;
		}

		void ShowTrailers()
		{
			ResetViews();
			GUIPropertyManager.SetProperty("#title", ptitle);
			tview = true;

			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(Trailers[i] != null)
			{
				GUIListItem item = new GUIListItem();
				item.Label = Trailers[i];
				item.IconImage = "defaultVideo.png";
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
			listview.Focus = true;
		}
		void ShowClips()
		{
			ResetViews();
			GUIPropertyManager.SetProperty("#title", ptitle);
			cview = true;

			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(Clips[i] != null)
			{
				GUIListItem item = new GUIListItem();
				item.Label = Clips[i];
				item.IconImage = "defaultVideo.png";
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
			listview.Focus = true;
		}
		void ShowMore()
		{
			ResetViews();
			GUIPropertyManager.SetProperty("#title", ptitle);
			mview = true;

			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(More[i] != null)
			{
				GUIListItem item = new GUIListItem();
				item.Label = More[i];
				item.IconImage = "defaultVideo.png";
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
			listview.Focus = true;
		}

		void ResetViews()
		{
			mainview = false;
			tcmview = false;
			tview = false;
			cview = false;
			mview = false;
			G_viewInfoAndTrailer = false;
			ShowLabelsFalse();
		}
	
		void GetMMSURL(string url)
		{
			TempHTML = "";
			GetWebPage(url, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			Match m = Regex.Match(TempHTML, @"<Ref\shref\s=\s.(?<mmsurl>mms.*).\s/>");
			MMSUrl = m.Groups["mmsurl"].Value;
			if(MMSUrl.Equals("")==true)
			{
				GUIDialogOK dlg = new GUIDialogOK();
				dlg.SetHeading(1025);
				dlg.SetLine(1, 5909);
				dlg.DoModal(GUIWindowManager.ActiveWindow);
			}
		}
		void Play(string sort)
		{
			GetGUIProperties();
			GetMMSURL(sort);
			GUIGraphicsContext.IsFullScreenVideo=true;
			GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
			g_Player.FullScreen=true;
			g_Player.Play(MMSUrl);

		}
		void ShowMovieInfo(string url, string name, int i)
		{
			TempHTML="";
			string coverurl;
			string PosterUrl="";
			string Plot="";
			string Genre="";
			string Runtime="";
			string ReleaseDate="";
			coverurl = "http://movies.yahoo.com/"+url;
			coverurl = coverurl.Replace("trailer","info");

			GetWebPage(coverurl, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			// Get PosterUrl
			Match m = Regex.Match(TempHTML,@"<img\ssrc=(?<posterurl>http://us.movies1.yimg.com/movies.yahoo.com/images/.*.jpg)");
			PosterUrl = m.Groups["posterurl"].Value;
			// If Cover is TV-serie example Desperate Housewives
			if(PosterUrl.Equals("")==true)
			{
				Match p = Regex.Match(TempHTML,@"<img\ssrc=(?<posterurl>http://shopping.yahoo.com/video/images/muze/dvd.*.jpg)");
				PosterUrl = p.Groups["posterurl"].Value;
			}

			using(WaitCursor cursor = new WaitCursor())
			{
				// Download Poster
				WebClient wc = new WebClient();
				name = name.Replace(":","-");
				wc.DownloadFile(PosterUrl, @"thumbs\MPTemp -"+name + ".jpg");

				while(System.IO.File.Exists(@"thumbs\MPTemp -"+name + ".jpg")!=true)
					GUIWindowManager.Process();

				// Display Poster
				poster.SetFileName(@"thumbs\MPTemp -"+name + ".jpg");
			}

			// Get MoviePlot
			int EP = TempHTML.IndexOf("<br clear=\"all\" />");
			int BP = TempHTML.LastIndexOf("size=-1>",EP)+8;
			int TP = EP-BP;

			Plot = TempHTML.Substring(BP,TP);
			GUIPropertyManager.SetProperty("#plot", Plot);
            
			// Get Genre
			Match m1 = Regex.Match(TempHTML,@"<b>Genres:\s</b>(?<genre>.*)");
			Genre = m1.Groups["genre"].Value;
			GUIPropertyManager.SetProperty("#genre", Genre);

			// Get Runtime
			Match m2 = Regex.Match(TempHTML,@"<b>Running\sTime:\s</b>(?<runtime>.*?)</font>");
			Runtime = m2.Groups["runtime"].Value;
			GUIPropertyManager.SetProperty("#runtime", Runtime);

			// Get ReleaseDate
			Match m3 = Regex.Match(TempHTML,@"<b>Release\sDate:.*;(?<releasedate>.*)");
			ReleaseDate = m3.Groups["releasedate"].Value;
			GUIPropertyManager.SetProperty("#year", ReleaseDate);

			// Get Rating
			Match r = Regex.Match(TempHTML,@"Average\sGrade.*.grade.>(?<rating>.*)<");
			switch(r.Groups["rating"].Value)
			{
				case "A+":
					prating = 10;
					return;
				case "A":
					prating = 9;
					return;
				case "A-":
					prating = 8.5;
					return;
				case "B+":
					prating = 8;
					return;
				case "B":
					prating = 7;
					return;
				case "B-":
					prating = 6.5;
					return;
				case "C+":
					prating = 6;
					return;
				case "C":
					prating = 5;
					return;
				case "C-":
					prating = 4.5;
					return;
				case "D+":
					prating = 4;
					return;
				case "D":
					prating = 3;
					return;
				case "D-":
					prating = 2.5;
					return;
				case "F":
					prating = 1;
					return;
			}
			GUIPropertyManager.SetProperty("#rating", prating.ToString());
			
			casturl = url;
			ShowLabelsTrue();
		}
		void GetJustAdded()
		{
			GetWebPage("http://movies.yahoo.com/trailers/", out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;
		
			int BJA = TempHTML.IndexOf(@"<!-- just added -->");
			int EJA = TempHTML.IndexOf(@"<!-- /just added -->",BJA);
			int TJA = EJA-BJA;

			string TempJA;
			TempJA = TempHTML.Substring(BJA,TJA);

			MatchCollection mc = Regex.Matches(TempJA,@"<a\shref=.(?<movieurl>.*).>(?<moviename>.*)</a>");

			int i = 0;

			foreach(Match m in mc)
			{
				JAMovieUrl[i] = m.Groups["movieurl"].Value;
				JAMovieName[i] = m.Groups["moviename"].Value;
				i++;
			}
		}
		void ShowJustAdded()
		{
			ResetViews();
			mwview = false;
			jaview = true;
			allview = false;
			letterview = false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\"+backgroundposter);
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5903));
			
			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(JAMovieName[i] !=null)
			{
				GUIListItem item = new GUIListItem();
				item.IsFolder = true;
				item.Label = JAMovieName[i];
				Utils.SetDefaultIcons(item);
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Movie_Selected;
			listview.Focus=true;
		}

		void GetMostWatched()
		{
			GetWebPage("http://movies.yahoo.com/trailers/", out TempHTML);
		
			if(TempHTML == null || TempHTML == string.Empty)
				return;

			int BMW = TempHTML.IndexOf(@"<!-- most watched -->");
			int EMW = TempHTML.IndexOf(@"<!-- /most watched -->",BMW);
			int TMW = EMW-BMW;

			string TempMW;
			TempMW = TempHTML.Substring(BMW,TMW);

			MatchCollection mc = Regex.Matches(TempMW,@"<a\shref=.(?<movieurl>.*).>(?<moviename>.*)</a>");

			int i = 0;

			foreach(Match m in mc)
			{
				MWMovieUrl[i] = m.Groups["movieurl"].Value;
				MWMovieName[i] = m.Groups["moviename"].Value;
				i++;
			}
		}
		void ShowMostWatched()
		{
			ResetViews();
			mwview = true;
			jaview = false;
			allview = false;
			letterview = false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\"+backgroundposter);
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5904));
			
			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(MWMovieName[i] !=null)
			{
				GUIListItem item = new GUIListItem();
				item.IsFolder = true;
				item.Label = MWMovieName[i];
				Utils.SetDefaultIcons(item);
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Movie_Selected;
		}

		void GetCastInfo(string url)
		{
			TempHTML="";
			string casturl;
			string cast = "";

			casturl = "http://movies.yahoo.com/"+url;
			casturl = casturl.Replace("trailer","cast");

			GetWebPage(casturl, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			MatchCollection mc = Regex.Matches(TempHTML, @"<A\sHRef=./shop.*>(?<cast>.*)</a>");

			foreach(Match m in mc)
			{
				if(cast.Equals(""))
				{
					cast = m.Groups["cast"].Value;
				}
				else
				{
					cast = cast.Insert(cast.Length,", ");
					cast = cast.Insert(cast.Length, m.Groups["cast"].Value);
				}
			}
			GUIPropertyManager.SetProperty("#cast", cast);
		}

		void ShowLabelsFalse()
		{
			label0.Visible=false;
			label1.Visible=false;
			label2.Visible=false;
			label3.Visible=false;
			label4.Visible=false;
			label5.Visible=false;
			label6.Visible=false;
			castarea.Visible=false;
			label8.Visible=false;
			plotarea.Visible=false;
			btntoggleplot.Visible=false;
			btntogglecast.Visible=false;
			//labelrating.Visible=false;
			btnletter.NavigateDown =4;

		}
		void ShowLabelsTrue()
		{
			label0.Visible=true;
			label1.Visible=true;
			label2.Visible=true;
			label3.Visible=true;
			label4.Visible=true;
			label5.Visible=true;
			label6.Visible=true;
			label8.Visible=true;
			btntoggleplot.Visible=true;
			btntogglecast.Visible=true;
			//labelrating.Visible=true;
			btnletter.NavigateDown =5;
			if(castview==true)
				ToggleButtonCast();
			else
				ToggleButtonPlot();
		}

		void GetGUIProperties()
		{
			ptitle = GUIPropertyManager.GetProperty("#title");
			pgenre = GUIPropertyManager.GetProperty("#genre");
			pruntime = GUIPropertyManager.GetProperty("#runtime");
			preleasedate = GUIPropertyManager.GetProperty("#year");
			pplot = GUIPropertyManager.GetProperty("#plot");
			pcast = GUIPropertyManager.GetProperty("#cast");
			//float.Parse(prating) = GUIPropertyManager.GetProperty("#rating");
		}
		void SetGUIProperties()
		{
			GUIPropertyManager.SetProperty("#title", ptitle);
			GUIPropertyManager.SetProperty("#genre", pgenre);
			GUIPropertyManager.SetProperty("#runtime", pruntime);
			GUIPropertyManager.SetProperty("#year", preleasedate);
			GUIPropertyManager.SetProperty("#plot", pplot);
			GUIPropertyManager.SetProperty("#cast", pcast);
			GUIPropertyManager.SetProperty("#rating", prating.ToString());
		}
		void LoadSettings()
		{
			if(plotview==true)
				ToggleButtonPlot();
			if(castview==true)
				ToggleButtonCast();
			if(tview==true)
				ShowTrailers();
			if(cview==true)
				ShowClips();
			if(mview==true)
				ShowMore();
			if(G_viewInfoAndTrailer==true)
				ShowGermanMovieInfo(GermanMovieName[GermanSelected], GermanSelected);
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml")) 
			{
				bitrate = xmlreader.GetValue("mytrailers","speed");
				Show_GT = xmlreader.GetValueAsBool("mytrailers","Show german trailers",false);
			}

		}


		#region German movie-trailers (de.movies.yahoo.com)
		//-------------------------------------------------------------------------	
		// German trailers de.movies.yahoo.com
		//-------------------------------------------------------------------------
		string[] GermanMovieName = new string[25];
		string[] GermanTrailerURL = new string[25];

		string[] G_Genre = new string[25];
		string[] G_Runtime = new string[25];
		string[] G_Releasedate = new string[25];
		string[] G_Plot = new string[25];
		string[] G_Cast = new string[25];
		string[] G_PosterUrl = new string[25];
		Double[] G_Rating = new double[25];

		int GermanSelected;
		
		bool G_viewWoche = false; //Woche listview
		bool G_viewMovie = false; //Movie listview
		bool G_viewInfoAndTrailer = false; // Info and Trailerview


		void GetGermanTrailers(string url)
		{
			Array.Clear(GermanMovieName, 0,25);
			Array.Clear(GermanTrailerURL, 0,25);
			Array.Clear(G_Genre, 0,25);
			Array.Clear(G_Runtime, 0,25);
			Array.Clear(G_Releasedate, 0,25);
			Array.Clear(G_Plot, 0,25);
			Array.Clear(G_Cast, 0,25);
			Array.Clear(G_PosterUrl, 0,25);
			Array.Clear(G_Rating, 0,25);

			GetWebPage(url, out TempHTML);
			if(TempHTML == null || TempHTML == string.Empty)
				return;

			// check if there are any trailersreviews available for this week;
            Match a = Regex.Match(TempHTML, @"Leider noch keine Film-Besprechungen");
			if(a.Success==true)
				return;
            
			string[] TempBlok = new string[25];

			MatchCollection mc = Regex.Matches(TempHTML, @"(?s-imnx:&nbsp;<span\sclass=.promo.(?<tempblok>.*?).E9EEF2.>)");
			int i = 0;
			foreach(Match m in mc)
			{
				//split page in bloks to ease search
				TempBlok[i] = m.Groups["tempblok"].Value;

				//get moviename & releasedate
				Match ur = Regex.Match(TempBlok[i], @"<a\shref=(?<movieurl>.*.html)>(?<moviename>.*)?</a>.*(?<releasedate>ab.*).</span>");
				GermanMovieName[i] = ur.Groups["moviename"].Value;
				ConvertStr(GermanMovieName[i], out GermanMovieName[i]);
				G_Releasedate[i] = ur.Groups["releasedate"].Value;

				// get posterurl
				Match p = Regex.Match(TempBlok[i], @"<IMG\sSRC=.*reviews/(?<posterurl>.*)s.jpg");
				if(p.Groups["posterurl"].Success==true)
					G_PosterUrl[i] = "http://eur.i1.yimg.com/eur.yimg.com/emvreviews/" + p.Groups["posterurl"].Value + "m.jpg";

				// get genre
				Match g = Regex.Match(TempBlok[i], @"Genre:</b>(?<moviegenre>.*)<br>");
				G_Genre[i] = g.Groups["moviegenre"].Value;
				ConvertStr(G_Genre[i], out G_Genre[i]);

				// get rating
				MatchCollection rc = Regex.Matches(TempBlok[i], @"http://eur.news1.yimg.com/eur.yimg.com/i/de/mo/1s.gif");
				G_Rating[i] = rc.Count;

				// get cast
				G_Cast[i]="";
				MatchCollection cc = Regex.Matches(TempBlok[i], @"http://de.search.movies.yahoo.com/search/movies_de.*<nobr>(?<cast>.*)</nobr>");
				foreach(Match c in cc)
				{
					if(G_Cast[i].Equals(""))
					{
						G_Cast[i] = c.Groups["cast"].Value;
					}
					else
					{
						G_Cast[i] = G_Cast[i].Insert(G_Cast[i].Length,", ");
						G_Cast[i] = G_Cast[i].Insert(G_Cast[i].Length, c.Groups["cast"].Value);
					}
				}
				ConvertStr(G_Cast[i], out G_Cast[i]);

				// get plot
				Match pl = Regex.Match(TempBlok[i], @"(?s-imnx:<td\scolspan.*class=.pcontent.>(?<movieplot>.*)<p\sstyle)");
				G_Plot[i] = pl.Groups["movieplot"].Value;
				ConvertStr(G_Plot[i], out G_Plot[i]);

				// get trailerurl
				Match t = Regex.Match(TempBlok[i], @"href=.(?<trailerurl>.*).><b>Trailer</b></a>");
				if(t.Groups["trailerurl"].Success==true)
					GermanTrailerURL[i] = "http://de.movies.yahoo.com" + t.Groups["trailerurl"].Value;
                
				i++;
			}
            
		}
		void ShowGermanLayoutWoche()
		{
			ResetViews();
			mwview = false;
			jaview = false;
			allview = false;
			letterview = false;
			G_viewWoche = true;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\"+backgroundposter);
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5911));

			listview.Clear();
			GUIListItem item0 = new GUIListItem();
			item0.Label = "..";
			item0.IsFolder = true;
			Utils.SetDefaultIcons(item0);
			listview.Add(item0);

			//add Vorwoche
			GUIListItem item = new GUIListItem();
			item.IsFolder = true;
			item.Label = GUILocalizeStrings.Get(5912);
			Utils.SetDefaultIcons(item);
			listview.Add(item);
			//add Diese Woche
			GUIListItem item1 = new GUIListItem();
			item1.IsFolder = true;
			item1.Label = GUILocalizeStrings.Get(5913);
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);
			// add Nachste Woche
			GUIListItem item2 = new GUIListItem();
			item2.IsFolder = true;
			item2.Label = GUILocalizeStrings.Get(5914);
			Utils.SetDefaultIcons(item2);
			listview.Add(item2);
			
			listview.SelectedListItemIndex = 1;
			listview.Focus=true;
			
		}
		void ShowGermanTrailers()
		{
			ResetViews();
			mwview = false;
			jaview = false;
			allview = false;
			letterview = false;
			G_viewWoche = false;
			G_viewMovie = true;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\"+backgroundposter);
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5911));
			
			listview.Clear();
			if(GermanMovieName[0]==null)
			{
				GUIListItem item = new GUIListItem();
				item.Label = ".. - " + GUILocalizeStrings.Get(5915);
				item.IsFolder = true;
				item.IconImage ="defaultFolderBack.png";
				listview.Add(item);
				return;
			}
			else
			{
				GUIListItem item1 = new GUIListItem();
				item1.Label = "..";
				item1.IsFolder = true;
				Utils.SetDefaultIcons(item1);
				listview.Add(item1);

				int i = 0;
				while(GermanMovieName[i] !=null)
				{
					GUIListItem item = new GUIListItem();
					item.IsFolder = true;
					item.Label = GermanMovieName[i];
					Utils.SetDefaultIcons(item);
					listview.Add(item);
					i++;
				}
				listview.SelectedListItemIndex = Movie_Selected;
				listview.Focus=true;
			}
		}
		void ShowGermanMovieInfo(string name, int i)
		{
			ResetViews();
			G_viewMovie=false;
			G_viewInfoAndTrailer=true;
			GermanSelected = i;
			Prev_SelectedItem = i+1;

			ShowLabelsTrue();
			GUIPropertyManager.SetProperty("#title", name);
			GUIPropertyManager.SetProperty("#genre", G_Genre[i]);
			label2.Visible=false; //runtime info not available
			GUIPropertyManager.SetProperty("#runtime", "");
			GUIPropertyManager.SetProperty("#year", G_Releasedate[i]);
			GUIPropertyManager.SetProperty("#plot", G_Plot[i]);
			GUIPropertyManager.SetProperty("#cast", G_Cast[i]);

			//Download Poster
			if(G_PosterUrl[i] !=null)
			{
				using(WaitCursor cursor = new WaitCursor())
				{
			
					// Download Poster
					WebClient wc = new WebClient();
					name = name.Replace(":","-");
					wc.DownloadFile(G_PosterUrl[i], @"thumbs\MPTemp -"+name + ".jpg");

					while(System.IO.File.Exists(@"thumbs\MPTemp -"+name + ".jpg")!=true)
						GUIWindowManager.Process();

					// Display Poster
					poster.SetFileName(@"thumbs\MPTemp -"+name + ".jpg");
				}
			}

			listview.Clear();
			if(GermanTrailerURL[i]==null)
			{
				GUIListItem item = new GUIListItem();
				item.Label = ".. - " + GUILocalizeStrings.Get(5916);
				item.IsFolder = true;
				item.IconImage ="defaultFolderBack.png";
				listview.Add(item);
			}
			else
			{
				GUIListItem item1 = new GUIListItem();
				item1.Label = "..";
				item1.IsFolder = true;
				Utils.SetDefaultIcons(item1);
				listview.Add(item1);

				GUIListItem item = new GUIListItem();
				item.Label = name + " Trailer";
				item.IsFolder = false;
				item.IconImage = "defaultVideo.png";
				listview.Add(item);
			}
		}
		void PlayGermanTrailer(string url)
		{
			GetWebPage(url, out TempHTML);

			Match m = Regex.Match(TempHTML, @"wmv-300-.\.(?<trailernumber>\d*)??,");
			string PlayUrl = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m.Groups["trailernumber"].Value + "&ru=y&b=8kd6ji91f1vde434d527d";
			Play(PlayUrl);
		}


		void ConvertStr(string input, out string output)
		{
			input = input.Replace("&uml;" ,((char)168).ToString());
			input = input.Replace("&Auml;" ,((char)196).ToString());
			input = input.Replace("&Euml;" ,((char)203).ToString());
			input = input.Replace("&Iuml;" ,((char)207).ToString());
			input = input.Replace("&Ouml;" ,((char)214).ToString());
			input = input.Replace("&Uuml;" ,((char)220).ToString());
			input = input.Replace("&auml;" ,((char)228).ToString());
			input = input.Replace("&euml;" ,((char)235).ToString());
			input = input.Replace("&iuml;" ,((char)239).ToString());
			input = input.Replace("&ouml;" ,((char)246).ToString());
			input = input.Replace("&uuml;" ,((char)252).ToString());
			input = input.Replace("&yuml;" ,((char)255).ToString());
			input = input.Replace("&Yuml;" ,((char)168).ToString());
			input = input.Replace("&szlig;", "�");
			output = input;
		}


		#endregion

	}
}


