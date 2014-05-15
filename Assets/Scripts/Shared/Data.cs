using UnityEngine;
using System.Collections.Generic;
using System;
using TVR.Helpers;

namespace TVR {	
	public static class Data {
		private static SqliteDatabase db = new SqliteDatabase();
		private static List<Chapter> mChapters;
		private static Chapter mSelChapter;

		public static Chapter selChapter {
			get { return mSelChapter; }
			set { mSelChapter = value; }
		}
		public static List<Chapter> Chapters {
			get { return mChapters; }
		}

		public static void Init() {
			createDataBase();
#if !UNITY_ANDROID
			db.ExecuteNonQuery("VACUUM");
#endif
			mChapters = new List<Chapter>();
			DataTable chapters = db.ExecuteQuery("SELECT IdChapter, Number, Title, Information, IdCharacter, IdBackground, IdMusic FROM Chapters ORDER BY Number");
			Chapter chapter;
			foreach(DataRow row in chapters.Rows) {
				chapter = new Chapter((int)row["IdChapter"], (int)row["Number"], (string)row["Title"], (string)row["Information"], (int)row["IdCharacter"], (int)row["IdBackground"], (int?)row["IdMusic"]);
				mChapters.Add(chapter);
			}
			mChapters.Sort();
		}
		
		private static void createDataBase() {
			bool exist = System.IO.File.Exists(Globals.DataBase);
			int VersionDB = 0;
			if(exist && Globals.CLEAR_DATA) {
				System.IO.File.Delete(Globals.DataBase);
				if(System.IO.Directory.Exists(Globals.RecordedSoundsPath))
					System.IO.Directory.Delete(Globals.RecordedSoundsPath, true);
				exist = false;
			}
			
			db.Open(Globals.DataBase);
			db.ExecuteNonQuery("PRAGMA foreign_keys=ON;");
			if(exist)
				VersionDB=(int)db.ExecuteQuery("pragma user_version;")[0]["user_version"];
			
			if(!System.IO.Directory.Exists(Globals.RecordedSoundsPath))
				System.IO.Directory.CreateDirectory(Globals.RecordedSoundsPath);
			
			if(VersionDB < 1) {
				db.ExecuteNonQuery("CREATE TABLE [BlockTypes] ([IdBlockType] INTEGER PRIMARY KEY NOT NULL UNIQUE, [BlockType] TEXT NOT NULL)");
				db.ExecuteNonQuery("INSERT INTO BlockTypes (IdBlockType, BlockType) VALUES (1, 'Time')");
				db.ExecuteNonQuery("INSERT INTO BlockTypes (IdBlockType, BlockType) VALUES (2, 'Voice')");

				db.ExecuteNonQuery("CREATE TABLE [ShotTypes] ([IdShotType] INTEGER PRIMARY KEY NOT NULL UNIQUE, [ShotType] TEXT NOT NULL)");
				db.ExecuteNonQuery("INSERT INTO ShotTypes (IdShotType, ShotType) VALUES (1, 'Close Up')");
				db.ExecuteNonQuery("INSERT INTO ShotTypes (IdShotType, ShotType) VALUES (2, 'Mid Shot')");
				db.ExecuteNonQuery("INSERT INTO ShotTypes (IdShotType, ShotType) VALUES (3, 'Long Shot')");

				db.ExecuteNonQuery("CREATE TABLE [FilterTypes] ([IdFilterType] INTEGER PRIMARY KEY NOT NULL UNIQUE, [FilterType] TEXT NOT NULL)");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (1, 'Off')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (2, 'Monster')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (3, 'Mosquito')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (4, 'Echo')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (5, 'Monster Pro')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (6, 'Mosquito Pro')");

				db.ExecuteNonQuery("CREATE TABLE [Chapters] ([IdChapter] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, [Number] INTEGER NOT NULL, [Title] TEXT NOT NULL, [Information] TEXT NOT NULL, [IdCharacter] INTEGER NOT NULL, [IdBackground] INTEGER NOT NULL, [IdMusic] INTEGER)");
				db.ExecuteNonQuery("CREATE TABLE [Blocks] ([IdBlock] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, [IdChapter] INTEGER NOT NULL REFERENCES [Chapters] ([IdChapter]) ON DELETE CASCADE, [IdBlockType] INTEGER NOT NULL REFERENCES [BlockTypes] ([IdBlockType]), [IdShotType] INTEGER NOT NULL REFERENCES [ShotTypes] ([IdShotType]), [IdFilterType] INTEGER NOT NULL REFERENCES [FilterTypes] ([IdFilterType]), [Number] INTEGER NOT NULL, [Frames] INTEGER NOT NULL, [IdExpression] INTEGER NOT NULL, [IdAnimation] INTEGER NOT NULL, [IdProp] INTEGER)");
				//db.ExecuteNonQuery("CREATE TABLE [CharacterProps] ([IdCharacterProps] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, [IdBlock] INTEGER NOT NULL REFERENCES [Blocks] ([IdBlock]) ON DELETE CASCADE, [IdResource] INTEGER NOT NULL, [Dummy] TEXT NOT NULL)");
				db.ExecuteQuery("pragma user_version=3;");
				VersionDB = 3;
			}
			if(VersionDB < 2) {
				db.ExecuteNonQuery("CREATE TABLE [FilterTypes] ([IdFilterType] INTEGER PRIMARY KEY NOT NULL UNIQUE, [FilterType] TEXT NOT NULL)");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (1, 'Off')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (2, 'Monster')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (3, 'Mosquito')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (4, 'Echo')");

				db.ExecuteNonQuery("ALTER TABLE Blocks ADD COLUMN [IdFilterType] INTEGER REFERENCES [FilterTypes] ([IdFilterType])");
				db.ExecuteNonQuery("UPDATE Blocks SET IdFilterType = 1");

				//db.ExecuteQuery("pragma user_version=2;");
			}
			if(VersionDB < 3) {
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (5, 'Monster Pro')");
				db.ExecuteNonQuery("INSERT INTO FilterTypes (IdFilterType, FilterType) VALUES (6, 'Mosquito Pro')");

				db.ExecuteQuery("pragma user_version=3;");
			}
		}
		
		public static void closeDB() {
#if !UNITY_ANDROID
			db.ExecuteNonQuery("VACUUM");
#endif
			db.Close();
			db = null;
		}

		public static Chapter newChapter(string title, string information, int idCharacter, int idBackground, int? idMusic) {
			return newChapter(mChapters.Count + 1, title, information, idCharacter, idBackground, idMusic);
		}

		public static Chapter newChapter(int number, string title, string information, int idCharacter, int idBackground, int? idMusic) {
			if(number <= 0 || number > mChapters.Count + 1)
				throw new System.Exception("The number must be between 0 and number of Chapters.");
			Chapter chapter = new Chapter(-1, number, title, information, idCharacter, idBackground, idMusic);
			chapter.Save();
			return chapter;
		}

		private static void renumberChapters(Chapter chapter, int oldNumber, int newNumber) {
			if(oldNumber == -1) {
				db.ExecuteNonQuery("UPDATE Chapters SET Number = Number + 1 WHERE IdChapter <> " + chapter.IdChapter + " AND Number >= " + newNumber);
				foreach(Chapter s in mChapters) {
					if(s != chapter && s.Number >= newNumber)
						s.forceNumber(s.Number + 1);
				}
			} else if(oldNumber > newNumber) {
				db.ExecuteNonQuery("UPDATE Chapters SET Number = Number + 1 WHERE IdChapter <> " + chapter.IdChapter + " AND Number >= " + newNumber + " AND Number < " + oldNumber);
				foreach(Chapter s in mChapters) {
					if(s != chapter && s.Number >= newNumber && s.Number < oldNumber)
						s.forceNumber(s.Number + 1);
				}
			} else if(oldNumber < newNumber) {
				db.ExecuteNonQuery("UPDATE Chapters SET Number = Number - 1 WHERE IdChapter <> " + chapter.IdChapter + " AND Number <= " + newNumber + " AND Number > " + oldNumber);
				foreach(Chapter s in mChapters) {
					if(s != chapter && s.Number <= newNumber && s.Number > oldNumber)
						s.forceNumber(s.Number - 1);
				}
			}
		}

		private static void addChapter(Chapter chapter) {
			if(!mChapters.Contains(chapter)) {
				mChapters.Add(chapter);
				if(chapter.Number < mChapters.Count - 1) {
					renumberChapters(chapter, -1, chapter.Number);
					mChapters.Sort();
				}
			}
		}

		private static void removeChapter(Chapter chapter) {
			if(mChapters.Contains(chapter))
				mChapters.Remove(chapter);
			db.ExecuteNonQuery("UPDATE Chapters SET Number = Number - 1 WHERE IdChapter <> " + chapter.IdChapter + " AND Number > " + chapter.Number);
			foreach(Chapter s in mChapters) {
				if(s != chapter && s.Number > chapter.Number)
					s.forceNumber(s.Number - 1);
			}
		}

		/*public static RecordedSound newRecordedSound(int number, string name, AudioClip audioClip, int FrequencyPlayback) {
			if(number>dicRecordedSounds.Count+1)
				throw new System.Exception("The number is greater than the number of items.");
			RecordedSound RS = new RecordedSound(-1,number,name,audioClip,FrequencyPlayback);
			RS.save();
			dicRecordedSounds.Add(RS.IdRecordedSound,RS);
			//mFolderVoices.addResource(RS.IdRecordedSound,"Scene/Library/Basic/nuevo_voz_button",RS.Name,"", RS.Number);
			//mFolderVoices.Content.Sort();
			return RS;
		}

		public static RecordedSound newRecordedSound(string name, AudioClip audioClip, int FrequencyPlayback) {
			if(mFolderVoices.Content.Count==0)
				return newRecordedSound(1, name, audioClip, FrequencyPlayback);
			else
				return newRecordedSound(mFolderVoices.Content[mFolderVoices.Content.Count-1].Number+1, name, audioClip, FrequencyPlayback);
		}*/

	public class Chapter : System.IComparable<Chapter>, iObject {
			private List<Block> mBlocks;
			private int mIdChapter;
			private int mNumber;
			private string mTitle;
			private string mInformation;
			private int mIdCharacter;
			private int mIdBackground;
			private int? mIdMusic;

			//private int mOldNumber;
			private string mOldTitle;
			private string mOldInformation;
			private int mOldIdCharacter;
			private int mOldIdBackground;
			private int? mOldIdMusic;
			//v2 = v1 ?? default(int);
			//v2 = v1 == null ? default(int) : v1;
			/*if(v1==null)
			   v2 = default(int);
			else
			   v2 = v1;*/
			//v2 = v1.GetValueOrDefault();
			//v2 = v1.GetValueOrDefault(-1);
			/*if(v1.HasValue)
   				v2 = v1.Value;*/

			public int IdChapter {
				get { return mIdChapter; }
			}
			public int Id {
				get { return mIdChapter; }
			}
			public int Number {
				get { return mNumber; }
			}
			public string Title {
				get { return mTitle; }
				set { mTitle = value; }
			}
			public string Information {
				get { return mInformation; }
				set { mInformation = value; }
			}
			public int IdCharacter {
				get { return mIdCharacter; }
				set { mIdCharacter = value; }
			}
			public int IdBackground {
				get { return mIdBackground; }
				set { mIdBackground = value; }
			}
			public int? IdMusic {
				get { return mIdMusic; }
				set { mIdMusic = value; }
			}
			public int IdMusicNotNullable {
				get { return mIdMusic ?? -1; }
				set {
					if(value < 0)
						mIdMusic = null;
					else
						mIdMusic = value;
				}
			}
			public List<Block> Blocks {
				get { return mBlocks; }
			}

			private Block mSelBlock;
			public Block selBlock {
				get { return mSelBlock; }
				set { mSelBlock = value; }
			}

			public Chapter(int idChapter, int number, string title, string information, int idCharacter, int idBackground, int? idMusic) {
				mIdChapter = idChapter;
				mNumber = number;
				mTitle = title;
				mInformation = information;
				mIdCharacter = idCharacter;
				mIdBackground = idBackground;
				mIdMusic = idMusic;
				
				mOldTitle = title;
				mOldInformation = information;
				mOldIdCharacter = idCharacter;
				mOldIdBackground = idBackground;
				mOldIdMusic = idMusic;
				mBlocks = new List<Block>();
			}

			public void Save() {
				if(mIdChapter == -1) {
					string idMus = mIdMusic == null ? "NULL" : mIdMusic.ToString();
					db.ExecuteNonQuery("INSERT INTO Chapters (Number, Title, Information, IdCharacter, IdBackground, IdMusic) VALUES (" + mNumber + ", '" + mTitle.Replace("'", "''").Trim() + "', '" + mInformation.Replace("'", "''").Trim() + "', " + mIdCharacter + ", " + mIdBackground + ", " + idMus + ")");
					mIdChapter = (int)db.ExecuteQuery("SELECT MAX(IdChapter) as ID FROM Chapters")[0]["ID"];
					Data.addChapter(this);
				} else {
					if(Change()) {
						string idMus = mIdMusic == null ? "NULL" : mIdMusic.ToString();
						db.ExecuteNonQuery("UPDATE Chapters SET Title = '" + Title.Replace("'", "''").Trim() + "', Information = '" + Information.Replace("'", "''").Trim() + "', IdCharacter = " + mIdCharacter + ", IdBackground = " + mIdBackground + ", IdMusic = " + idMus + " WHERE IdChapter = " + mIdChapter);
						mOldTitle = mTitle;
						mOldInformation = mInformation;
						mOldIdBackground = mIdBackground; 
						mOldIdCharacter = mIdCharacter;
						mOldIdMusic = mIdMusic;
					}
				}
			}

			public bool Change() {
				return mTitle != mOldTitle || mInformation != mOldInformation || mIdBackground != mOldIdBackground || mIdCharacter != mOldIdCharacter || mIdMusic != mOldIdMusic;
			}

			public void Delete() {
				db.ExecuteNonQuery("DELETE FROM Chapters WHERE IdChapter = " + mIdChapter);
				Data.removeChapter(this);
			}
			
			public void Renumber(int newNumber) {
				if(mNumber != newNumber) {
					if(newNumber <= 0 || newNumber > Data.mChapters.Count)
						throw new System.Exception("The new number must be between 0 and Data.Chapters.Count.");
					db.ExecuteNonQuery("UPDATE Chapters SET Number = " + newNumber + " WHERE IdChapter = " + mIdChapter);
					Data.renumberChapters(this, mNumber, newNumber);
					Data.mChapters.Sort();
					mNumber = newNumber;
				}
			}

			public void forceNumber(int newNumber) {
				if(mNumber != newNumber) {
					if(newNumber <= 0 || newNumber > Data.mChapters.Count)
						throw new System.Exception("The new number must be between 0 and number of Chapters.");
					mNumber = newNumber;
				}
			}

			public Block newBlock(Block.blockTypes BlockType, Block.shotTypes ShotType, Block.filterType FilterType, int frames, int idExpression, int idAnimation, int? idProp) {
				return newBlock(mBlocks.Count + 1, BlockType, ShotType, FilterType, frames, idExpression, idAnimation, idProp);
			}

			public Block newBlock(int number, Block.blockTypes BlockType, Block.shotTypes ShotType, Block.filterType FilterType, int frames, int idExpression, int idAnimation, int? idProp) {
				if(number <= 0 || number > mBlocks.Count + 1)
					throw new System.Exception("The number must be between 0 and number of Blocks in this chapter.");
				Block block = new Block(-1, BlockType, ShotType, FilterType, number, frames, idExpression, idAnimation, idProp, this);
				block.Save();
				return block;
			}

			public void loadBlocks() {
				if(mBlocks != null) {
					mBlocks = new List<Block>();
					DataTable blocks = db.ExecuteQuery("SELECT IdBlock, IdBlockType, IdShotType, IdFilterType, Number, Frames, IdExpression, IdAnimation, IdProp FROM Blocks WHERE IdChapter = " + mIdChapter + " ORDER BY Number");
					Block block;
					foreach(DataRow row in blocks.Rows) {
						block = new Block((int)row["IdBlock"], (Block.blockTypes)row["IdBlockType"], (Block.shotTypes)row["IdShotType"], (Block.filterType)row["IdFilterType"], (int)row["Number"], (int)row["Frames"], (int)row["IdExpression"], (int)row["IdAnimation"], (int?)row["IdProp"], this);
						mBlocks.Add(block);
					}
					mBlocks.Sort();
					foreach(Block b in mBlocks)
						b.loadResource();
				}
			}

			public void unloadBlocks() {
				mSelBlock = null;
				foreach(Block b in mBlocks)
					b.unloadResource();
			}

			private void renumberBlocks(Block block, int oldNumber, int newNumber) {
				if(oldNumber == -1) {
					db.ExecuteNonQuery("UPDATE Blocks SET Number = Number + 1 WHERE IdBlock <> " + block.IdBlock + " AND IdChapter = " + block.IdChapter + " AND Number >= " + newNumber);
					foreach(Block b in mBlocks) {
						if(b != block && b.Number >= newNumber)
							b.forceNumber(b.Number + 1);
					}
				} else if(oldNumber > newNumber) {
					db.ExecuteNonQuery("UPDATE Blocks SET Number = Number + 1 WHERE IdBlock <> " + block.IdBlock + " AND IdChapter = " + block.IdChapter + " AND Number >= " + newNumber + " AND Number < " + oldNumber);
					foreach(Block b in mBlocks) {
						if(b != block && b.Number >= newNumber && b.Number < oldNumber)
							b.forceNumber(b.Number + 1);
					}
				} else if(oldNumber < newNumber) {
					db.ExecuteNonQuery("UPDATE Blocks SET Number = Number - 1 WHERE IdBlock <> " + block.IdBlock + " AND IdChapter = " + block.IdChapter + " AND Number <= " + newNumber + " AND Number > " + oldNumber);
					foreach(Block b in mBlocks) {
						if(b != block && b.Number <= newNumber && b.Number > oldNumber)
							b.forceNumber(b.Number - 1);
					}
				}
			}

			private void addBlock(Block block) {
				if(!mBlocks.Contains(block)) {
					mBlocks.Add(block);
					if(block.Number < mBlocks.Count - 1) {
						renumberBlocks(block, -1, block.Number);
						mBlocks.Sort();
					}
				}
			}

			private void removeBlock(Block block) {
				if(mBlocks.Contains(block))
					mBlocks.Remove(block);
				db.ExecuteNonQuery("UPDATE Blocks SET Number = Number - 1 WHERE IdBlock <> " + block.IdBlock + " AND IdChapter = " + block.IdChapter + " AND Number > " + block.Number);
				foreach(Block b in mBlocks) {
					if(b != block && b.Number > block.Number)
						b.forceNumber(b.Number - 1);
				}
			}

			public void Revert() {
				mTitle = mOldTitle;
				mInformation = mOldInformation;
				mIdBackground = mOldIdBackground; 
				mIdCharacter = mOldIdCharacter;
				mIdMusic = mOldIdMusic;
			}

			public int CompareTo(Chapter other) {		
				if(this.Number < other.Number)
					return -1;
				else if(this.Number > other.Number)
						return 1;
					else
						return 0;
			}

			public class Block : System.IComparable<Block>, iObject {
				public enum blockTypes {
					Time = 1,
					Voice = 2,
				}
				public enum shotTypes {
					CloseUP = 1,
					MidShot = 2,
					LongShot = 3,
				}
				public enum filterType {
					Off = 1,
					Monster = 2,
					Mosquito = 3,
					Echo = 4,
					MonsterPro = 5,
					MosquitoPro = 6,
				}

				private Chapter mParent;
				private int mIdBlock;
				private blockTypes mBlockType;
				private shotTypes mShotType;
				private filterType mFilterType;
				private int mNumber;
				private int mFrames;
				private int mIdExpression;
				private int mIdAnimation;
				private int? mIdProp;

				private blockTypes mOldBlockType;
				private shotTypes mOldShotType;
				private filterType mOldFilterType;
				private int mOldFrames;
				private int mOldIdExpression;
				private int mOldIdAnimation;
				private int? mOldIdProp;

				public Chapter Parent {
					get { return mParent; }
				}
				public int IdBlock {
					get { return mIdBlock; }
				}
				public int Id {
					get { return mIdBlock; }
				}
				public int IdChapter {
					get { return mParent.mIdChapter; }
				}
				public blockTypes BlockType {
					get { return mBlockType; }
					set { mBlockType = value; }
				}
				public shotTypes ShotType {
					get { return mShotType; }
					set { mShotType = value; }
				}
				public filterType FilterType {
					get { return mFilterType; }
					set { mFilterType = value; }
				}
				public int Number {
					get { return mNumber; }
				}
				public int Frames {
					get { return mFrames; }
					set { mFrames = value; }
				}
				public int IdExpression {
					get { return mIdExpression; }
					set { mIdExpression = value; }
				}
				public int IdAnimation {
					get { return mIdAnimation; }
					set { mIdAnimation = value; }
				}
				public int? IdProp {
					get { return mIdProp; }
					set { mIdProp = value; }
				}
				public int IdPropNotNullable {
					get { return mIdProp ?? -1; }
					set {
						if(value < 0)
							mIdProp = null;
						else
							mIdProp = value;
					}
				}

				public Block(int idBlock, blockTypes BlockType, shotTypes ShotType, filterType FilterType, int number, int frames, int idExpression, int idAnimation, int? idProp, Chapter parent) {
					mParent = parent;
					mIdBlock = idBlock;
					mBlockType = BlockType;
					mShotType = ShotType;
					mFilterType = FilterType;
					mNumber = number;
					mFrames = frames;
					mIdExpression = idExpression;
					mIdAnimation = idAnimation;
					mIdProp = idProp;

					mOldBlockType = BlockType;
					mOldShotType = ShotType;
					mOldFilterType = FilterType;
					mOldFrames = frames;
					mOldIdExpression = idExpression;
					mOldIdAnimation = idAnimation;
					mOldIdProp = idProp;
				}

				public void Save() {
					if(mIdBlock == -1) {
						string idProp = mIdProp == null ? "NULL" : mIdProp.ToString();
						db.ExecuteNonQuery("INSERT INTO Blocks (IdChapter, IdBlockType, IdShotType, IdFilterType, Number, Frames, IdExpression, IdAnimation, IdProp) VALUES (" + mParent.mIdChapter + ", " + (int)mBlockType + ", " + (int)mShotType + ", " + (int)mFilterType + ", " + mNumber + ", " + mFrames + ", " + mIdExpression + ", " + mIdAnimation + ", " + idProp + ")");
						mIdBlock = (int)db.ExecuteQuery("SELECT MAX(IdBlock) as ID FROM Blocks")[0]["ID"];
						mParent.addBlock(this);
					} else {
						SaveSound();
						if(Change()) {
							string idProp = mIdProp == null ? "NULL" : mIdProp.ToString();
							db.ExecuteNonQuery("UPDATE Blocks SET IdBlockType = " + (int)mBlockType + ", IdShotType = " + (int)mShotType + ", IdFilterType = " + (int)mFilterType + ", Frames = " + mFrames + ", IdExpression = " + mIdExpression + ", IdAnimation = " + mIdAnimation + ", IdProp = " + idProp + " WHERE IdBlock = " + mIdBlock);
							mOldBlockType = mBlockType;
							mOldShotType = mShotType;
							mOldFilterType = mFilterType;
							mOldFrames = mFrames;
							mOldIdExpression = mIdExpression;
							mOldIdAnimation = mIdAnimation;
						}
					}
				}

				public bool Change() {
					return ChangeBBDD() || ChangeSound();
				}

				private bool ChangeBBDD() {
					return mBlockType != mOldBlockType || mShotType != mOldShotType || mFilterType != mOldFilterType || mFrames != mOldFrames || mIdExpression != mOldIdExpression || mIdAnimation != mOldIdAnimation || mIdProp != mOldIdProp;
				}

				public void Delete() {
					db.ExecuteNonQuery("DELETE FROM Blocks WHERE IdBlock = " + mIdBlock);
					mParent.removeBlock(this);
					if(mBlockType == blockTypes.Voice) {
						//if(mSound != null && mSound == mOldSound)
						DeleteSound();
						unloadResource();
						/*if(mSound != null)
							MonoBehaviour.DestroyImmediate(mSound);
						if(mOldSound != null && mSound != mOldSound)
							MonoBehaviour.DestroyImmediate(mOldSound);
						mSound = null;
						mOldSound = null;
						mSoundLoaded = false;*/
					}
				}

				public void Renumber(int newNumber) {
					if(mNumber != newNumber) {
						if(newNumber <= 0 || newNumber > mParent.mBlocks.Count)
							throw new System.Exception("The new number must be between 0 and mParent.Blocks.Count.");
						db.ExecuteNonQuery("UPDATE Blocks SET Number = " + newNumber + " WHERE IdBlock = " + mIdBlock);
						mParent.renumberBlocks(this, mNumber, newNumber);
						mParent.mBlocks.Sort();
						mNumber = newNumber;
					}
				}

				public void forceNumber(int newNumber) {
					if(mNumber != newNumber) {
						if(newNumber <= 0 || newNumber > mParent.mBlocks.Count)
							throw new System.Exception("The new number must be between 0 and number of Blocks.");
						mNumber = newNumber;
					}
				}

				public void Revert() {
					mOldBlockType = mBlockType;
					mOldShotType = mShotType;
					mOldFilterType = mFilterType;
					mOldFrames = mFrames;
					mOldIdExpression = mIdExpression;
					mOldIdAnimation = mIdAnimation;
					mOldIdProp = mIdProp;
					if(mSound != mOldSound) {
						MonoBehaviour.DestroyImmediate(mSound);
						mSound = mOldSound;
					}
					if(mOriginalSound != mOldOriginalSound) {
						MonoBehaviour.DestroyImmediate(mOriginalSound);
						mOriginalSound = mOldSound;
					}
				}

				public int CompareTo(Block other) {		
					if(this.Number < other.Number)
						return -1;
					else if(this.Number > other.Number)
						return 1;
					else
						return 0;
				}

				#region Sound
				public const int FREQUENCY = Globals.OUTPUTRATEPERSECOND;  
				public const string EXTENSION = ".xjc";
				public const string ORIGINAL = "_Original";
				private AudioClip mSound;
				private AudioClip mOldSound;
				private bool mSoundLoaded;
				private QueueManager.QueueManagerAction mActionLoad1;
				private QueueManager.QueueManagerAction mActionLoad2;

				private AudioClip mOriginalSound;
				private AudioClip mOldOriginalSound;
				private bool mOriginalSoundLoaded;
				private QueueManager.QueueManagerAction mActionOriginalLoad1;
				private QueueManager.QueueManagerAction mActionOriginalLoad2;

				public AudioClip Sound {
					set {
						if(mBlockType == blockTypes.Voice) {
							if(value == null) {
								/*if(mSound != null)
									MonoBehaviour.DestroyImmediate(mSound);
								mSoundLoaded = false;*/
								throw new Exception("You can't assign a null sound.");
							} else
								mSoundLoaded = true;
							if(mSound != mOldSound)
								MonoBehaviour.DestroyImmediate(mSound);
							mSound = value;
						} else
							throw new Exception("Are you trying to load an audioclip into block time?");
					}
					get {
						if(mBlockType == blockTypes.Voice) {
							if(mActionLoad1 != null) {
								System.Threading.Thread t = (System.Threading.Thread)mActionLoad1.function.Target;
								if(t.IsAlive) {
									while(t.IsAlive)
										System.Threading.Thread.Sleep(100);
								} else {
									QueueManager.removeAction(mActionLoad1);
									LoadSound();
									return mSound;
								}
							}
							if(mActionLoad2 != null) {
								QueueManager.removeAction(mActionLoad2);
								mActionLoad2.function.Invoke();
							}

							if(!mSoundLoaded)
								LoadSound();
							return mSound;
						} else
							return null;
					}
				}

				public AudioClip OriginalSound {
					set {
						if(mBlockType == blockTypes.Voice) {
							if(value == null) {
								throw new Exception("You can't assign a null sound.");
							} else
								mOriginalSoundLoaded = true;
							if(mOriginalSound != mOldSound)
								MonoBehaviour.DestroyImmediate(mOriginalSound);
							mOriginalSound = value;
						} else
							throw new Exception("Are you trying to load an audioclip into block time?");
					}
					get {
						if(mBlockType == blockTypes.Voice) {
							if(mActionOriginalLoad1 != null) {
								System.Threading.Thread t = (System.Threading.Thread)mActionOriginalLoad1.function.Target;
								if(t.IsAlive) {
									while(t.IsAlive)
										System.Threading.Thread.Sleep(100);
								} else {
									QueueManager.removeAction(mActionOriginalLoad1);
									LoadOriginalSound();
									return mOriginalSound;
								}
							}
							if(mActionOriginalLoad2 != null) {
								QueueManager.removeAction(mActionOriginalLoad2);
								mActionOriginalLoad2.function.Invoke();
							}

							if(!mOriginalSoundLoaded)
								LoadOriginalSound();
							return mOriginalSound;
						} else
							return null;
					}
				}

				public void loadResource() {
					if(mBlockType == blockTypes.Voice) {
						//mSoundLoaded = false;
						mActionLoad2 = null;
						if(!mSoundLoaded) {
							System.Threading.Thread t = new System.Threading.Thread(() => LoadSoundAsync1());
							mActionLoad1 = new QueueManager.QueueManagerAction("LoadAudioClip", t.Start, "RecordedSound.LoadSoundAsync1");
							QueueManager.add(mActionLoad1, QueueManager.Priorities.High);
						}

						//mOriginalSoundLoaded = false;
						mActionOriginalLoad2 = null;
						if(!mOriginalSoundLoaded) {
							System.Threading.Thread t = new System.Threading.Thread(() => LoadOriginalSoundAsync1());
							mActionOriginalLoad1 = new QueueManager.QueueManagerAction("LoadAudioClip", t.Start, "RecordedSound.LoadSoundAsync1");
							QueueManager.add(mActionOriginalLoad1, QueueManager.Priorities.Low);
						}
					}
				}

				public void unloadResource() {
					if(mSound != null)
						MonoBehaviour.DestroyImmediate(mSound);
					if(mOldSound != null && mSound != mOldSound)
						MonoBehaviour.DestroyImmediate(mOldSound);
					mSound = null;
					mOldSound = null;
					mSoundLoaded = false;

					if(mOriginalSound != null)
						MonoBehaviour.DestroyImmediate(mOriginalSound);
					if(mOldOriginalSound != null && mOriginalSound != mOldOriginalSound)
						MonoBehaviour.DestroyImmediate(mOldOriginalSound);
					mOriginalSound = null;
					mOldOriginalSound = null;
					mOriginalSoundLoaded = false;
				}

				private bool ChangeSound() {
					return mSound != mOldSound || mOriginalSound != mOldOriginalSound;
				}

				private void LoadSound() {
					if(!mSoundLoaded) {
						string filePath = System.IO.Path.Combine(Globals.RecordedSoundsPath, mIdBlock + EXTENSION);
						if(System.IO.File.Exists(filePath)) {
							byte[] byteArray = null;
							byteArray = LZ4Sharp.LZ4.Decompress(filePath);
							short[] sSamples = new short[byteArray.Length / sizeof(short)];
							Buffer.BlockCopy(byteArray, 0, sSamples, 0, byteArray.Length);
							float[] samples = new float[sSamples.Length];
							for(int i = 0; i < sSamples.Length; ++i)
								samples[i] = ((float)sSamples[i]) / short.MaxValue;

							mSound = AudioClip.Create("user_clip", samples.Length, 1, FREQUENCY, false, false);
							mSound.SetData(samples, 0);
						} else {
							mSound = AudioClip.Create("user_clip", 0, 1, FREQUENCY, false, false);
							//mSound = AudioClip.Create("user_clip", FREQUENCY, 1, FREQUENCY, false, false);
						}
						mOldSound = mSound;
						mSoundLoaded = true;
					}
				}

				private void LoadOriginalSound() {
					if(!mOriginalSoundLoaded) {
						string filePath = System.IO.Path.Combine(Globals.RecordedSoundsPath, mIdBlock + ORIGINAL + EXTENSION);
						if(System.IO.File.Exists(filePath)) {
							byte[] byteArray = null;
							byteArray = LZ4Sharp.LZ4.Decompress(filePath);
							short[] sSamples = new short[byteArray.Length / sizeof(short)];
							Buffer.BlockCopy(byteArray, 0, sSamples, 0, byteArray.Length);
							float[] samples = new float[sSamples.Length];
							for(int i = 0; i < sSamples.Length; ++i)
								samples[i] = ((float)sSamples[i]) / short.MaxValue;

							mOriginalSound = AudioClip.Create("user_clip", samples.Length, 1, FREQUENCY, false, false);
							mOriginalSound.SetData(samples, 0);
						} else {
							mSound = AudioClip.Create("user_clip", 0, 1, FREQUENCY, false, false);
							//mSound = AudioClip.Create("user_clip", FREQUENCY, 1, FREQUENCY, false, false);
						}
						mOldOriginalSound = mOriginalSound;
						mOriginalSoundLoaded = true;
					}
				}

				private void LoadSoundAsync1() {
					if(!mSoundLoaded) {
						string filePath = System.IO.Path.Combine(Globals.RecordedSoundsPath, mIdBlock + EXTENSION);
						if(System.IO.File.Exists(filePath)) {
							byte[] byteArray = null;
							byteArray = LZ4Sharp.LZ4.Decompress(filePath);
							short[] sSamples = new short[byteArray.Length / sizeof(short)];
							Buffer.BlockCopy(byteArray, 0, sSamples, 0, byteArray.Length);
							float[] samples = new float[sSamples.Length];
							for(int i = 0; i < sSamples.Length; ++i)
								samples[i] = ((float)sSamples[i]) / short.MaxValue;

							mActionLoad2 = new QueueManager.QueueManagerAction("LoadAudioClip", () => LoadSoundAsync2(samples), "RecordedSound.LoadSoundAsync2");
							QueueManager.add(mActionLoad2, QueueManager.Priorities.Highest);
						} else {
							mActionLoad2 = new QueueManager.QueueManagerAction("LoadAudioClip", () => LoadSoundAsync2(null), "RecordedSound.LoadSoundAsync2");
							QueueManager.add(mActionLoad2, QueueManager.Priorities.Highest);
						}
					}
					mActionLoad1 = null;
				}

				private void LoadSoundAsync2(float[] samples) {
					if(samples != null) {
						mSound = AudioClip.Create("user_clip", samples.Length, 1, FREQUENCY, false, false);
						mSound.SetData(samples, 0);
					} else {
						mSound = AudioClip.Create("user_clip", 0, 1, FREQUENCY, false, false);
						//mSound = AudioClip.Create("user_clip", FREQUENCY, 1, FREQUENCY, false, false);
					}
					mOldSound = mSound;
					mSoundLoaded = true;
					mActionLoad2 = null;
					samples = null;
				}

				private void LoadOriginalSoundAsync1() {
					if(!mOriginalSoundLoaded) {
						string filePath = System.IO.Path.Combine(Globals.RecordedSoundsPath, mIdBlock + ORIGINAL + EXTENSION);
						if(System.IO.File.Exists(filePath)) {
							byte[] byteArray = null;
							byteArray = LZ4Sharp.LZ4.Decompress(filePath);
							short[] sSamples = new short[byteArray.Length / sizeof(short)];
							Buffer.BlockCopy(byteArray, 0, sSamples, 0, byteArray.Length);
							float[] samples = new float[sSamples.Length];
							for(int i = 0; i < sSamples.Length; ++i)
								samples[i] = ((float)sSamples[i]) / short.MaxValue;

							mActionOriginalLoad2 = new QueueManager.QueueManagerAction("LoadAudioClip", () => LoadOriginalSoundAsync2(samples), "RecordedSound.LoadSoundAsync2");
							QueueManager.add(mActionOriginalLoad2, QueueManager.Priorities.Highest);
						} else {
							mActionOriginalLoad2 = new QueueManager.QueueManagerAction("LoadAudioClip", () => LoadOriginalSoundAsync2(null), "RecordedSound.LoadSoundAsync2");
							QueueManager.add(mActionOriginalLoad2, QueueManager.Priorities.Highest);
						}
					}
					mActionOriginalLoad1 = null;
				}

				private void LoadOriginalSoundAsync2(float[] samples) {
					if(samples != null) {
						mOriginalSound = AudioClip.Create("user_clip", samples.Length, 1, FREQUENCY, false, false);
						mOriginalSound.SetData(samples, 0);
					} else {
						mSound = AudioClip.Create("user_clip", 0, 1, FREQUENCY, false, false);
						//mSound = AudioClip.Create("user_clip", FREQUENCY, 1, FREQUENCY, false, false);
					}
					mOldOriginalSound = mOriginalSound;
					mOriginalSoundLoaded = true;
					mActionOriginalLoad2 = null;
					samples = null;
				}

				private void SaveSound() {
					if(mOldBlockType == blockTypes.Voice && mBlockType != blockTypes.Voice) {
						//if(mSound != null && mSound == mOldSound)
						DeleteSound();
						unloadResource();
					} else if(mBlockType == blockTypes.Voice) {
						float[] samples = null;
						float[] samplesOriginal = null;
						if(mSound != mOldSound && mSoundLoaded) {
							samples = new float[mSound.samples * mSound.channels];
							mSound.GetData(samples, 0);
							if(mOldSound != null)
								MonoBehaviour.DestroyImmediate(mOldSound);
							mOldSound = mSound;
							/*System.Threading.Thread t = new System.Threading.Thread(() => SaveSound(samples));
							t.Start();*/
						}
						if(mOriginalSound != mOldOriginalSound && mOriginalSoundLoaded) {
							samplesOriginal = new float[mOriginalSound.samples * mOriginalSound.channels];
							mOriginalSound.GetData(samplesOriginal, 0);
							if(mOldOriginalSound != null)
								MonoBehaviour.DestroyImmediate(mOldOriginalSound);
							mOldOriginalSound = mOriginalSound;
							/*System.Threading.Thread t = new System.Threading.Thread(() => SaveOriginalSound(samplesOriginal));
							t.Start();*/
						}
						System.Threading.Thread t = new System.Threading.Thread(() => SaveSound(samples, samplesOriginal));
						t.Start();
					}
				}

				private void SaveSound(float[] samples, float[] samplesOriginal) {
					Debug.Log("Start");
					Utils.AudioFilters filter = new TVR.Utils.AudioFilters();
					/*float[] outSamples;
					filter.Mosquito(samples, out outSamples);*/
					string filePath;
					if(samples != null) {
						filePath = System.IO.Path.Combine(Globals.RecordedSoundsPath, mIdBlock + EXTENSION);
						//SaveSound(outSamples, filePath);
						SaveSound(samples, filePath);
					}
					if(samplesOriginal != null) {
						filePath = System.IO.Path.Combine(Globals.RecordedSoundsPath, mIdBlock + ORIGINAL + EXTENSION);
						SaveSound(samplesOriginal, filePath);
					}
					Debug.Log("End");
				}

				private void SaveSound(float[] samples, string filePath) {
					//string filePath = System.IO.Path.Combine(Globals.RecordedSoundsPath, mIdBlock + EXTENSION);
					short[] sSamples = new short[samples.Length];
					for(int i = 0; i < samples.Length; ++i)
						sSamples[i] = (short)Mathf.RoundToInt(samples[i] * short.MaxValue);

					byte[] byteArray = new byte[sSamples.Length * sizeof(short)];
					Buffer.BlockCopy(sSamples, 0, byteArray, 0, byteArray.Length);
					LZ4Sharp.LZ4.Compress(filePath, byteArray);
				}

				private void DeleteSound() {
					if(System.IO.File.Exists(System.IO.Path.Combine(TVR.Globals.RecordedSoundsPath, mIdBlock + EXTENSION)))
						System.IO.File.Delete(System.IO.Path.Combine(TVR.Globals.RecordedSoundsPath, mIdBlock + EXTENSION));
					if(System.IO.File.Exists(System.IO.Path.Combine(TVR.Globals.RecordedSoundsPath, mIdBlock + ORIGINAL + EXTENSION)))
						System.IO.File.Delete(System.IO.Path.Combine(TVR.Globals.RecordedSoundsPath, mIdBlock + ORIGINAL + EXTENSION));
				}
				#endregion
			}
		}

		/*public class Episode : System.IComparable<Episode> {
			private int mIdEpisode;
			internal int mNumber;
			private int mOldNumber;
			public string Title;
			public string Information;
			private string mOldTitle;
			private string mOldInformation;
			private int mDuration;
			public Dictionary<int, Scene> Scenes;
			
			public int IdEpisode {
				get { return mIdEpisode; }
			}
			public int Number {
				get { return mNumber; }
			}
			public int oldNumber {
				get { return mOldNumber; }
			}
			public int DurationInFrames {
				get { return mDuration; }
			}
			public float DurationInSeconds {
				get { return mDuration/BRBRec.Globals.FRAMESPERSECOND; }
			}
			
			internal Episode(int IdEpisode, int number, string title, string info) {
				mIdEpisode=IdEpisode;
				mNumber=number;
				Title=title;
				Information=info;
				mOldTitle=title;
				mOldInformation=info;
				mDuration=0;
				mVersion=1;
				mEvents = new List<CVS>();
			}
			
			public void loadScenes() {
				if(Scenes==null) {
					Scenes=new Dictionary<int, Scene>();
					DataTable scenes = db.ExecuteQuery("SELECT Scenes.IdScene, Number, Title, Information FROM Scenes INNER JOIN ScenesInfo ON Scenes.IdScene = ScenesInfo.IdScene WHERE IdEpisode = " + mIdEpisode);
					Scene scene;
					foreach(DataRow row in scenes.Rows) {
						scene = new Scene((int)row["IdScene"], (int)row["Number"], (string)row["Title"], (string)row["Information"], this);
						Scenes.Add (scene.IdScene,scene);
						scene.change+=changeScene;
					}
					updateDuration();
				}
			}
			
			public Scene newScene(int number, string title, string info) {
				if(number>Scenes.Count+1)
					throw new System.Exception("The number is greater than the number of items.");
				Scene scene = new Scene(-1,number,title,info, this);
				scene.change+=changeScene;
				scene.save();
				Scenes.Add (scene.IdScene,scene);
				return scene;
			}
	
			private void updateDuration() {
				mDuration=(int)db.ExecuteQuery("SELECT SUM(Frames) as Duration FROM Scenes INNER JOIN Stages ON Scenes.IdScene = Stages.IdScene INNER JOIN StagesInfo ON Stages.IdStage = StagesInfo.IdStage " +
					"WHERE Stages.CVSDel IS NULL AND StagesInfo.CVSDel IS NULL AND Scenes.CVSDel IS NULL AND IdEpisode = " + mIdEpisode)[0]["Duration"];
			}
			
			public void delete() {
				db.ExecuteNonQuery("UPDATE Episodes SET CVSDel = " + Data.Version + " WHERE IdEpisode = " + mIdEpisode + " AND CVSDel IS NULL");
				CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Delete, this);
				OnChange(evt);
			}
			
			public void saveAudioClips(string path, System.IO.StreamWriter log, Export_Main export) {
				byte[] emptySamplesBytes = null;
				if(DurationInFrames * Globals.MILISPERFRAME < 15)
					emptySamplesBytes = new byte[DurationInFrames * Globals.NUMCHANNELS * Globals.OUTPUTRATEPERFRAME * sizeof(short)];
				//log.WriteLine("Exportando episodio " + mNumber + ".");
				int offset = 0;
				int samples = DurationInFrames * Globals.OUTPUTRATEPERFRAME;
				foreach(Scene scene in Scenes.Values) {
					scene.loadStages();
					scene.saveAudioClips(samples, emptySamplesBytes, offset, path, log, export);
					offset += scene.DurationInFrames;
				}
			}

			public int CompareTo(Episode other) {		
				if ( this.Number < other.Number ) return -1;
				else if ( this.Number > other.Number ) return 1;
				else return 0;
			}

			public class Scene : CVS.iCVSObject, System.IComparable<Scene> {
				private int mIdScene;
				internal int mNumber;
				private int mOldNumber;
				public string Title;
				public string Information;
				private string mOldTitle;
				private string mOldInformation;
				private int mDuration;
				public Dictionary<int, Stage> Stages;
				private Episode mParent;
				//private List<int> mListTexLoading;

				public delegate void undoDelegate(CVS.CVSTypes Type, Stage stage);
				private undoDelegate mUndoDelegate;
				
				private int mVersion;
				private List<CVS> mEvents;
				//internal event changeEvent change;
				public int Version {
					get { return mVersion; }
				}

				public int IdScene {
					get { return mIdScene; }
				}
				public int IdEpisode {
					get { return mParent.IdEpisode; }
				}
				public int Number {
					get { return mNumber; }
				}
				public int oldNumber {
					get { return mOldNumber; }
				}
				public int episodeNumber {
					get { return mParent.Number; }
				}
				public int DurationInFrames {
					get {
						loadStages();
						return mDuration;
					}
				}
				public float DurationInSeconds {
					get {
						loadStages();
						return mDuration/BRBRec.Globals.FRAMESPERSECOND;
					}
				}
				
				internal Scene(int IdScene, int number, string title, string info, Episode parent) {
					mIdScene=IdScene;
					mNumber=number;
					Title=title;
					Information = info;
					mOldTitle=title;
					mOldInformation=info;
					mVersion=1;
					mEvents = new List<CVS>();
					mParent=parent;
				}
				
				public void loadStages() {
					if(Stages==null) {
						Stages=new Dictionary<int, Stage>();
						DataTable stages = db.ExecuteQuery("SELECT Stages.IdStage, Number, Title, Information, Frames, IdObject, IdResource FROM Stages INNER JOIN StagesInfo ON Stages.IdStage = StagesInfo.IdStage INNER JOIN Objects ON Stages.IdStage = Objects.IdStage WHERE IdScene = " + mIdScene + " AND IdObjectType  = " + (int)Stage.StageObject.Types.Background + " ORDER BY Number");
						Stage stage;
						foreach(DataRow row in stages.Rows) {
							stage = new Stage((int)row["IdStage"], (int)row["Number"], (string)row["Title"], (string)row["Information"], (int)row["Frames"], this, (int)row["IdObject"], (int)row["IdResource"]);
							Stages.Add (stage.IdStage,stage);
							stage.change+=changeStage;
						}
						SceneMgr.Get().StartCoroutine(loadTexScreenshot());
					}
					updateDuration();
				}
				
				private System.Collections.IEnumerator loadTexScreenshot() {
					yield return new WaitForSeconds(1f);
					foreach(Stage stage in Stages.Values) {
						foreach(Stage.Screenshot screen in stage.Screenshots) {
							if(System.IO.File.Exists(System.IO.Path.Combine(Globals.ScreenshotsPath, screen.IdScreenshot + ".png"))) {
								WWW texToLoad = new WWW("file://" + System.IO.Path.Combine(Globals.ScreenshotsPath, screen.IdScreenshot + ".png"));
								yield return new WaitForEndOfFrame();
								yield return texToLoad;
								screen.texScreen = new Texture2D(Globals.SCREENSHOT_WIDTH, Globals.SCREENSHOT_HEIGHT);
								texToLoad.LoadImageIntoTexture(screen.texScreen);
								yield return new WaitForEndOfFrame();
								yield return new WaitForEndOfFrame();
								texToLoad.Dispose();
							}
						}
					}
				}
				
				public Stage newStage(int number, string title, string info, int frames) {
					if(number>Stages.Count+1)
						throw new System.Exception("The number is greater than the number of items.");
					Stage stage = new Stage(-1,number,title,info,frames,this);
					stage.change+=changeStage;
					stage.save();
					Stages.Add(stage.IdStage,stage);
					return stage;
				}

				public Stage duplicateStage(int number, Stage dStage, bool complete) {
					if(number > Stages.Count + 1)
						throw new System.Exception("The number is greater than the number of items.");
					Stage stage = new Stage(-1, number, dStage.Title, dStage.Information, dStage.Frames, this);
					stage.change += changeStage;
					stage.save(false);
					stage.copyStage(dStage, complete);
					Stages.Add(stage.IdStage, stage);
					stage.Background.loadObject();
					return stage;
				}

				private void changeStage(List<CVS.iCVSObject> sender, CVS.CVSEvent eventArgs) {
					if(eventArgs.Modified is Stage) {
						Stage stage = (Stage)eventArgs.Modified;
						CVS cvs = new CVS(mVersion, sender, eventArgs.Type, eventArgs.Modified);
						switch(eventArgs.Type) {
						case CVS.CVSTypes.Info:
							mEvents.Add(cvs);
							break;
						case CVS.CVSTypes.Renumbered:
							mEvents.Add(cvs);
							int Value;
							int first;
							int last;
							if(stage.Number>stage.oldNumber) {
								Value=-1;
								first=stage.oldNumber;
								last=stage.Number;
							} else {
								Value=1;
								first=stage.Number;
								last=stage.oldNumber;
							}
							renumberStages(stage,first,last,Value);
							break;
						case CVS.CVSTypes.Create:
							mEvents.Add(cvs);
							renumberStages(stage,stage.Number,-1,1);
							break;
						case CVS.CVSTypes.Delete:
							updateDuration();
							CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.UpdateDuration, this);
							OnChange (evt);
							
							stage.change-=changeStage;
							Stages.Remove(stage.IdStage);
							mEvents.Add(cvs);
							renumberStages(stage,stage.Number,-1,-1);
							break;
						}
						mVersion++;
					}
					//if((eventArgs.Modified == null || eventArgs.Modified is Scene.Stage) && eventArgs.Type != Data.CVS.CVSTypes.Create) {
					if(eventArgs.Type == Data.CVS.CVSTypes.UpdateDuration) {
						updateDuration();
					}
					OnChange(sender, eventArgs);
				}

				private void updateDuration() {
					mDuration=0;
					foreach(Stage stage in Stages.Values) {
						mDuration += stage.Frames;
					}
				}
				
				public void delete() {
					db.ExecuteNonQuery("UPDATE Scenes SET CVSDel = " + mParent.Version + " WHERE IdScene = " + mIdScene + " AND CVSDel IS NULL");
					CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Delete, this);
					OnChange(evt);
				}
				
				public void save() {
					if(mIdScene==-1) {
						db.ExecuteNonQuery("INSERT INTO Scenes (IdEpisode, CVSNew) VALUES (" + mParent.IdEpisode + ", " + mParent.Version + ")");
						mIdScene=(int)db.ExecuteQuery("SELECT MAX(IdScene) as ID FROM Scenes")[0]["ID"];
						db.ExecuteNonQuery("UPDATE ScenesInfo SET Number = " + mNumber + ", Title = '" + Title.Replace("'","''").Trim() + "', Information = '" + Information.Replace("'","''").Trim() + "' WHERE IdScene = " + mIdScene + " AND CVSNew = -1");
						CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Create, this);
						OnChange(evt);
					} else {
						if(mOldInformation==Information && mOldTitle==Title)
							return;
						db.ExecuteNonQuery("UPDATE ScenesInfo SET CVSDel = " + mParent.Version + " WHERE IdScene = " + mIdScene + " AND CVSDel IS NULL");
						db.ExecuteNonQuery("INSERT INTO ScenesInfo (IdScene, CVSNew, Number, Title, Information) VALUES ("+ mIdScene +", "+ mParent.Version +", "+ mNumber +", '"+ Title.Replace("'","''").Trim() +"', '"+ Information.Replace("'","''").Trim() +"')");
						CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Info, this);
						OnChange (evt);
						mOldTitle=Title;
						mOldInformation=Information;
					}
				}
	
				public void Renumber(int number) {
					if(number>mParent.Scenes.Count)
						throw new System.Exception("The number is greater than the number of items.");
					if(mNumber!=number) {
						mOldNumber=mNumber;
						mNumber=number;
						db.ExecuteNonQuery("UPDATE ScenesInfo SET CVSDel = " + mParent.Version + " WHERE IdScene = " + mIdScene + " AND CVSDel IS NULL");
						db.ExecuteNonQuery("INSERT INTO ScenesInfo (IdScene, CVSNew, Number, Title, Information) VALUES ("+ mIdScene +", "+ mParent.Version +", "+ mNumber +", '"+ mOldTitle.Replace("'","''").Trim() +"', '"+ mOldInformation.Replace("'","''").Trim() +"')");
						CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Renumbered, this);
						OnChange (evt);
					}
				}
				
				private void renumberStages(Stage stageCompare, int firstNumber, int lastNumber, int Value) {
					if(lastNumber==-1)
						lastNumber=1000000;
				
					foreach(Stage stage in Stages.Values) {
						if(stage.Number>=firstNumber && stage.Number<=lastNumber && stage!=stageCompare) {
							stage.mNumber+=Value;
							db.ExecuteNonQuery("UPDATE StagesInfo SET Number = " + stage.Number + " WHERE IdStage = " + stage.IdStage + " AND CVSDel IS NULL");
						}
					}
				}
	
				internal override bool undoLocal(CVS cvs) {
					switch(cvs.Type) {
					case CVS.CVSTypes.Create:
						db.ExecuteNonQuery("DELETE FROM Scenes WHERE IdScene = " + mIdScene + " AND CVSNew = " + cvs.Version);
						return true;
					case CVS.CVSTypes.Delete:
						db.ExecuteNonQuery("UPDATE Scenes SET CVSDel = NULL WHERE IdScene = " + mIdScene + " AND CVSDel = " + cvs.Version);
						return true;
					case CVS.CVSTypes.Renumbered:
						int sceneNumber = (int)db.ExecuteQuery("SELECT Number FROM ScenesInfo WHERE IdScene = " + mIdScene + " AND CVSDel = " + cvs.Version)[0]["Number"];
						db.ExecuteNonQuery("DELETE FROM ScenesInfo WHERE IdScene = " + mIdScene + " AND CVSDel IS NULL");
						db.ExecuteNonQuery("UPDATE ScenesInfo SET CVSDel = NULL WHERE IdScene = " + mIdScene + " AND CVSDel = " + cvs.Version);
						mOldNumber=mNumber;
						mNumber=sceneNumber;
						return true;
					case CVS.CVSTypes.Info:
						DataRow row = db.ExecuteQuery("SELECT Title, Information FROM ScenesInfo WHERE IdScene = " + mIdScene + " AND CVSDel = " + cvs.Version)[0];
						Title=(string)row["Title"];
						mOldTitle=(string)row["Title"];
						Information=(string)row["Information"];
						mOldInformation=(string)row["Information"];
						db.ExecuteNonQuery("DELETE FROM ScenesInfo WHERE IdScene = " + mIdScene + " AND CVSDel IS NULL");
						db.ExecuteNonQuery("UPDATE ScenesInfo SET CVSDel = NULL WHERE IdScene = " + mIdScene + " AND CVSDel = " + cvs.Version);
						return true;
					}
					return false;
				}
				
				public int undoCounts() {
					return mEvents.Count;
				}
		
				public void undo(undoDelegate undoDel) {
					if(mEvents.Count==0)
						return;
					mUndoDelegate = undoDel;
					Stage stage;
					CVS cvs = mEvents[mEvents.Count-1];
					switch(cvs.Type) {
					case CVS.CVSTypes.Create:
						BRB.Utils.Message.Show(0,Texts.WARNING,Texts.SCENE_UNDO_CREATE,BRB.Utils.Message.Type.YesNo,Texts.YES,Texts.NO, undoMessageCreate);
						return;
					case CVS.CVSTypes.Delete:
						stage = (Stage)cvs.Modified;
						if(stage.undoLocal(cvs)) {
							updateDuration();
							CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.UpdateDuration, this);
							OnChange (evt);
							
							stage.change+=changeStage;
							Stages.Add(stage.IdStage, stage);
							renumberStages(stage,stage.Number,-1,1);
							
							DataTable stages = db.ExecuteQuery("SELECT DISTINCT BlocksTimeline.IdResource AS IdRecordedSound FROM BlocksTimeline INNER JOIN Objects ON BlocksTimeline.IdObject = Objects.IdObject " +
								"INNER JOIN Stages ON Objects.IdStage = Stages.IdStage WHERE IdBlockType = 5 AND BlocksTimeline.CVSDel IS NULL AND BlocksTimeline.IdResource < 0 AND Objects.CVSDel IS NULL AND Stages.IdStage = " + stage.IdStage);
							foreach(DataRow row in stages.Rows) {
								Data.undoRS((int)row["IdRecordedSound"]);
							}

							mEvents.Remove (cvs);
							if(mUndoDelegate!=null)
								mUndoDelegate(CVS.CVSTypes.Delete,stage);
							return;
						}
						break;
					case CVS.CVSTypes.Renumbered:
						stage = (Stage)cvs.Modified;
						if(stage.undoLocal(cvs)) {
							int Value;
							int first;
							int last;
							if(stage.Number>stage.oldNumber) {
								Value=-1;
								first=stage.oldNumber;
								last=stage.Number;
							} else {
								Value=1;
								first=stage.Number;
								last=stage.oldNumber;
							}
							renumberStages(stage,first,last,Value);
		
							mEvents.Remove (cvs);
							if(mUndoDelegate!=null)
								mUndoDelegate(CVS.CVSTypes.Renumbered,stage);
							return;
						}
						break;
					case CVS.CVSTypes.Info:
						stage = (Stage)cvs.Modified;
						if(stage.undoLocal(cvs)) {
							mEvents.Remove (cvs);
							if(mUndoDelegate!=null)
								mUndoDelegate(CVS.CVSTypes.Info,stage);
							return;
						}
						break;
					}
					if(mUndoDelegate!=null)
						mUndoDelegate(CVS.CVSTypes.Nothing,null);
				}
				
				private void undoMessageCreate(BRB.Utils.Message.ButtonClicked buttonClicked, int Identifier) {
					if(buttonClicked==BRB.Utils.Message.ButtonClicked.Yes) {
						CVS cvs = mEvents[mEvents.Count-1];
						Stage stage = (Stage)cvs.Modified;
						if(stage.undoLocal(cvs)) {
							stage.change-=changeStage;
							Stages.Remove(stage.IdStage);
							renumberStages(stage,stage.Number,-1,-1);
							mEvents.Remove(cvs);
							if(mUndoDelegate!=null)
								mUndoDelegate(CVS.CVSTypes.Create,stage);
							return;
						}
					}
					if(mUndoDelegate!=null)
						mUndoDelegate(CVS.CVSTypes.Nothing,null);
				}

				public void saveAudioClips(string path, System.IO.StreamWriter log, Export_Main export) {
					byte[] emptySamplesBytes = null;
					if(DurationInFrames * Globals.MILISPERFRAME < 15)
						emptySamplesBytes = new byte[DurationInFrames * Globals.NUMCHANNELS * Globals.OUTPUTRATEPERFRAME * sizeof(short)];
					saveAudioClips(DurationInFrames * Globals.OUTPUTRATEPERFRAME, emptySamplesBytes, 0, path, log, export);
				}

				internal void saveAudioClips(int samples, byte[] emptySamplesBytes, int framesOffset, string path, System.IO.StreamWriter log, Export_Main export) {
					//log.WriteLine("Exportando secuencia " + mNumber + ".");
					int offset = framesOffset;
					foreach(Stage sta in Stages.Values) {
						sta.loadStage();
						sta.saveAudioClips(samples, emptySamplesBytes, offset, path, log, export);
						offset += sta.DurationInFrames;
					}
				}

				public int CompareTo(Scene other) {		
					if ( this.Number < other.Number ) return -1;
					else if ( this.Number > other.Number ) return 1;
					else return 0;
				}

				public class Stage : CVS.iCVSObject, System.IComparable<Stage> {
					private int mIdStage;
					internal int mNumber;
					private int mOldNumber;
					public string Title;
					public string Information;
					private int mFrames; //No se pueden deshacer la primera vez que se asignan.
					private string mOldTitle;
					private string mOldInformation;
					private int mOldFrames;
					private Scene mParent;
					public List<Screenshot> Screenshots;
					public Dictionary<int, StageObject> Objects;
					
					private int mVersion;
					private List<CVS> mEvents;
					//internal event changeEvent change;
					public int Version {
						get { return mVersion; }
					}

					public int IdStage {
						get { return mIdStage; }
					}
					public int IdScene {
						get { return mParent.IdScene; }
					}
					public int Number {
						get { return mNumber; }
					}
					public int oldNumber {
						get { return mOldNumber; }
					}
					public int episodeNumber {
						get { return mParent.episodeNumber; }
					}
					public int sceneNumber {
						get { return mParent.Number; }
					}
					public int DurationInFrames {
						get { return mFrames; }
					}
					public float DurationInSeconds {
						get { return (float)mFrames/(float)BRBRec.Globals.FRAMESPERSECOND; }
					}
					public int Frames {
						get { return mFrames; }
						set {
							if(mFrames == 0)
								mFrames = value;
							else
								throw new System.Exception("To change the number of frames you must use redimFrames function.");
						}
					}
					public int minFrames(bool before) {
						int ret;
						if(!before) {
							ret = Globals.MINFRAMES;
							DataTable dt = db.ExecuteQuery("SELECT IFNULL(MAX(Frame), " + Globals.MINFRAMES + ") AS EndFrame FROM KeyFramesInfo INNER JOIN KeyFrames ON KeyFramesInfo.IdKeyFrame = KeyFrames.IdKeyFrame INNER JOIN Objects ON KeyFrames.IdObject = Objects.IdObject " +
								"WHERE KeyFramesInfo.CVSDel IS NULL AND KeyFrames.CVSDel IS NULL AND Objects.CVSDel IS NULL AND Objects.IdStage = " + mIdStage);
							if(dt.Rows.Count > 0 && (int)dt.Rows[0]["EndFrame"] > ret)
								ret = (int)dt.Rows[0]["EndFrame"];
							
							dt = db.ExecuteQuery("SELECT IFNULL(MAX(StartFrame + Frames), " + Globals.MINFRAMES + ") AS EndFrame FROM BlocksTimelineInfo INNER JOIN BlocksTimeline ON BlocksTimelineInfo.IdBlockTimeline = BlocksTimeline.IdBlockTimeline " +
								"INNER JOIN Objects ON BlocksTimeline.IdObject = Objects.IdObject WHERE BlocksTimelineInfo.CVSDel IS NULL AND BlocksTimeline.CVSDel IS NULL AND Objects.CVSDel IS NULL AND Objects.IdStage = " + mIdStage);
							if(dt.Rows.Count > 0 && dt.Rows[0]["EndFrame"] is int && (int)dt.Rows[0]["EndFrame"] > ret)
								ret = (int)dt.Rows[0]["EndFrame"];
						} else {
							ret = Globals.MAXFRAMES;
							DataTable dt = db.ExecuteQuery("SELECT IFNULL(MIN(Frame), " + Globals.MAXFRAMES + ") AS EndFrame FROM KeyFramesInfo INNER JOIN KeyFrames ON KeyFramesInfo.IdKeyFrame = KeyFrames.IdKeyFrame INNER JOIN Objects ON KeyFrames.IdObject = Objects.IdObject " +
								"WHERE KeyFramesInfo.CVSDel IS NULL AND KeyFrames.CVSDel IS NULL AND Objects.CVSDel IS NULL AND Objects.IdStage = " + mIdStage + " AND Frame > -1");
							if(dt.Rows.Count > 0 && (int)dt.Rows[0]["EndFrame"] < ret)
								ret = (int)dt.Rows[0]["EndFrame"];

							dt = db.ExecuteQuery("SELECT IFNULL(MIN(StartFrame), " + Globals.MAXFRAMES + ") AS EndFrame FROM BlocksTimelineInfo INNER JOIN BlocksTimeline ON BlocksTimelineInfo.IdBlockTimeline = BlocksTimeline.IdBlockTimeline " +
								"INNER JOIN Objects ON BlocksTimeline.IdObject = Objects.IdObject WHERE BlocksTimelineInfo.CVSDel IS NULL AND BlocksTimeline.CVSDel IS NULL AND Objects.CVSDel IS NULL AND Objects.IdStage = " + mIdStage);
							if(dt.Rows.Count > 0 && dt.Rows[0]["EndFrame"] is int && (int)dt.Rows[0]["EndFrame"] < ret)
								ret = (int)dt.Rows[0]["EndFrame"];
							ret = mFrames - ret;
						}
						return ret;
					}
					
					public StageObject Background;
					public StageObject SceneCamera;
					public StageObject selObject;
					
					internal float MiliSeconds;

					private bool mBatchStarted;
					
					public delegate void undoDelegate(CVS.CVSTypes Type, object modified, StageObject parent);
					internal undoDelegate mUndoDelegate;

					internal Stage(int IdStage, int number, string title, string info, int frames, Scene parent) {
						mIdStage=IdStage;
						mNumber=number;
						Title=title;
						Information=info;
						mFrames=frames;
						//IdBackground=idBackground;
						mOldTitle=title;
						mOldInformation=info;
						mOldFrames=mFrames;
						mVersion=1;
						mEvents = new List<CVS>();
						mParent=parent;
						
						Screenshots=new List<Screenshot>();
						DataTable scenes = db.ExecuteQuery("SELECT Screenshots.IdScreenshot, Number FROM Screenshots INNER JOIN ScreenshotsInfo ON Screenshots.IdScreenshot = ScreenshotsInfo.IdScreenshot WHERE IdStage = " + mIdStage + " ORDER BY Number");
						Screenshot screen;
						foreach(DataRow row in scenes.Rows) {
							screen = new Screenshot((int)row["IdScreenshot"], (int)row["Number"], this);
							Screenshots.Add(screen);
							screen.change+=changeScreenshot;
						}
						mBatchStarted = false;
						//Screenshots.Sort();
					}
					
					internal Stage(int IdStage, int number, string title, string info, int frames, Scene parent, int idBackground, int idResourceBackground) : this(IdStage, number, title, info, frames, parent) {
						if(idBackground<0)
							throw new System.Exception("The idBackground must be a valid Id.");
						Background = new StageObject(idBackground,StageObject.Types.Background,idResourceBackground, this);
						mBatchStarted = false;
					}

					internal void copyStage(Stage cStage, bool complete) {
						Background.IdResource = cStage.Background.IdResource;
						//Duplicate screenshots
						int idTemp;
						if(complete) {
							string filePath;
							string filePath2;
							Screenshot screen;
							foreach(Stage.Screenshot screenShot in cStage.Screenshots) {
								db.ExecuteNonQuery("INSERT INTO Screenshots (IdStage, CVSNew) VALUES (" + mIdStage + ", " + -1 + ")");
								idTemp = (int)db.ExecuteQuery("SELECT MAX(IdScreenshot) as ID FROM Screenshots")[0]["ID"];
								db.ExecuteNonQuery("UPDATE ScreenshotsInfo SET Number = " + screenShot.Number + " WHERE IdScreenshot = " + idTemp + " AND CVSNew = -1");

								filePath = System.IO.Path.Combine(Globals.ScreenshotsPath, screenShot.IdScreenshot + ".png");
								filePath2 = System.IO.Path.Combine(Globals.ScreenshotsPath, idTemp + ".png");
								if(System.IO.File.Exists(filePath))
									System.IO.File.Copy(filePath, filePath2);

								screen = new Screenshot(idTemp, screenShot.Number, this);
								Screenshots.Add(screen);
								screen.change += changeScreenshot;
								//SceneMgr.Get().StartCoroutine(loadTexScreenshot());
							}
						}

						//Duplicate objects
						DataTable dt;
						int idObject;
						int IdType;
						DataTable objects = db.ExecuteQuery("SELECT IdObject, IdObjectType, IdResource FROM Objects WHERE IdStage = " + cStage.IdStage + " AND CVSDel IS NULL");
						foreach(DataRow row in objects.Rows) {
							if((int)row["IdObjectType"] <= 2) {
								idObject = (int)db.ExecuteQuery("SELECT IdObject FROM Objects WHERE IdStage = " + mIdStage + " AND IdObjectType = " + (int)row["IdObjectType"])[0]["IdObject"];
							} else {
								db.ExecuteNonQuery("INSERT INTO Objects (IdStage, IdObjectType, CVSNew, IdResource) VALUES (" + mIdStage + ", " + (int)row["IdObjectType"] + ", " + -1 + ", " + (int)row["IdResource"] + ")");
								idObject = (int)db.ExecuteQuery("SELECT MAX(IdObject) as ID FROM Objects")[0]["ID"];
							}

							if(complete) {
								//Duplicar completo
								//KeyFrames
								dt = db.ExecuteQuery("SELECT IdKeyFrameType, Frame, X, Y, Z, Control FROM KeyFrames INNER JOIN KeyFramesInfo ON KeyFrames.IdKeyFrame = KeyFramesInfo.IdKeyFrame WHERE IdObject = " + (int)row["IdObject"] + " AND KeyFrames.CVSDel IS NULL AND KeyFramesInfo.CVSDel IS NULL");
								foreach(DataRow row1 in dt.Rows) {
									db.ExecuteNonQuery("INSERT INTO KeyFrames (IdObject, IdKeyFrameType, CVSNew) VALUES (" + idObject + ", " + (int)row1["IdKeyFrameType"] + ", -1)");
									idTemp = (int)db.ExecuteQuery("SELECT MAX(IdKeyFrame) as ID FROM KeyFrames")[0]["ID"];
									db.ExecuteNonQuery("UPDATE KeyFramesInfo SET Frame = " + (int)row1["Frame"] + ", X = " + System.Convert.ToSingle(row1["X"]) + ", Y = " + System.Convert.ToSingle(row1["Y"]) + ", Z = " + System.Convert.ToSingle(row1["Z"]) + ", Control = " + (int)row1["Control"] + " WHERE IdKeyFrame = " + idTemp + " AND CVSNew = -1");
								}

								//Blocks
								dt = db.ExecuteQuery("SELECT BlocksTimeline.IdBlockTimeline, IdBlockType, IdResource, StartFrame, Frames, Dummy, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, ScaleX, ScaleY, ScaleZ, Control FROM BlocksTimeline INNER JOIN BlocksTimelineInfo ON BlocksTimeline.IdBlockTimeline = BlocksTimelineInfo.IdBlockTimeline LEFT OUTER JOIN BlocksTimelineProp ON BlocksTimeline.IdBlockTimeline = BlocksTimelineProp.IdBlockTimeline WHERE IdObject = " + (int)row["IdObject"] + " AND BlocksTimeline.CVSDel IS NULL AND BlocksTimelineInfo.CVSDel IS NULL AND BlocksTimelineProp.CVSDel IS NULL");
								foreach(DataRow row2 in dt.Rows) {
									db.ExecuteNonQuery("INSERT INTO BlocksTimeline (IdObject, IdBlockType, CVSNew, IdResource) VALUES (" + idObject + ", " + (int)row2["IdBlockType"] + ", -1, " + (int)row2["IdResource"] + ")");
									idTemp = (int)db.ExecuteQuery("SELECT MAX(IdBlockTimeline) as ID FROM BlocksTimeline")[0]["ID"];
									db.ExecuteNonQuery("UPDATE BlocksTimelineInfo SET StartFrame = " + (int)row2["StartFrame"] + ", Frames = " + (int)row2["Frames"] + ", Control = " + (int)row2["Control"] + " WHERE IdBlockTimeline = " + idTemp + " AND CVSNew = -1");
									if((StageObject.BlockTimeline.Types)row2["IdBlockType"] == StageObject.BlockTimeline.Types.Prop1 || (StageObject.BlockTimeline.Types)row2["IdBlockType"] == StageObject.BlockTimeline.Types.Prop2 || (StageObject.BlockTimeline.Types)row2["IdBlockType"] == StageObject.BlockTimeline.Types.Prop3)
										db.ExecuteNonQuery("INSERT INTO BlocksTimelineProp (IdBlockTimeline, CVSNew, Dummy, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, ScaleX, ScaleY, ScaleZ) VALUES (" + idTemp + ", -1, '" + (string)row2["Dummy"] + "', " + System.Convert.ToSingle(row2["PositionX"]) + ", " + System.Convert.ToSingle(row2["PositionY"]) + ", " + System.Convert.ToSingle(row2["PositionZ"]) + ", " + System.Convert.ToSingle(row2["RotationX"]) + ", " + System.Convert.ToSingle(row2["RotationY"]) + ", " + System.Convert.ToSingle(row2["RotationZ"]) + ", " + System.Convert.ToSingle(row2["ScaleX"]) + ", " + System.Convert.ToSingle(row2["ScaleY"]) + ", " + System.Convert.ToSingle(row2["ScaleZ"]) + ")");
								}
							} else {
								//Duplicar como plantilla
								//KeyFrames
								IdType = 1;
								dt = db.ExecuteQuery("SELECT IdKeyFrameType, Frame, X, Y, Z FROM KeyFrames INNER JOIN KeyFramesInfo ON KeyFrames.IdKeyFrame = KeyFramesInfo.IdKeyFrame WHERE IdObject = " + (int)row["IdObject"] + " AND KeyFrames.CVSDel IS NULL AND KeyFramesInfo.CVSDel IS NULL ORDER BY IdKeyFrameType, Frame DESC");
								foreach(DataRow row1 in dt.Rows) {
									if(IdType == (int)row1["IdKeyFrameType"]) {
										db.ExecuteNonQuery("INSERT INTO KeyFrames (IdObject, IdKeyFrameType, CVSNew) VALUES (" + idObject + ", " + (int)row1["IdKeyFrameType"] + ", -1)");
										idTemp = (int)db.ExecuteQuery("SELECT MAX(IdKeyFrame) as ID FROM KeyFrames")[0]["ID"];
										db.ExecuteNonQuery("UPDATE KeyFramesInfo SET Frame = -1, X = " + System.Convert.ToSingle(row1["X"]) + ", Y = " + System.Convert.ToSingle(row1["Y"]) + ", Z = " + System.Convert.ToSingle(row1["Z"]) + ", Control = 0 WHERE IdKeyFrame = " + idTemp + " AND CVSNew = -1");
										if((int)row1["Frame"] != -1) {
											db.ExecuteNonQuery("INSERT INTO KeyFrames (IdObject, IdKeyFrameType, CVSNew) VALUES (" + idObject + ", " + (int)row1["IdKeyFrameType"] + ", -1)");
											idTemp = (int)db.ExecuteQuery("SELECT MAX(IdKeyFrame) as ID FROM KeyFrames")[0]["ID"];
											db.ExecuteNonQuery("UPDATE KeyFramesInfo SET Frame = 0, X = " + System.Convert.ToSingle(row1["X"]) + ", Y = " + System.Convert.ToSingle(row1["Y"]) + ", Z = " + System.Convert.ToSingle(row1["Z"]) + ", Control = 0 WHERE IdKeyFrame = " + idTemp + " AND CVSNew = -1");
										}
										IdType++;
									}
								}

								//Visibility Block
								if((int)row["IdObjectType"] > 2) {
									db.ExecuteNonQuery("INSERT INTO BlocksTimeline (IdObject, IdBlockType, CVSNew, IdResource) VALUES (" + idObject + ", 1, -1, -1)");
									idTemp = (int)db.ExecuteQuery("SELECT MAX(IdBlockTimeline) as ID FROM BlocksTimeline")[0]["ID"];
									db.ExecuteNonQuery("UPDATE BlocksTimelineInfo SET StartFrame = 0, Frames = " + mFrames + ", Control = -1 WHERE IdBlockTimeline = " + idTemp + " AND CVSNew = -1");
								}
							}
						}
					}

					public void loadScreenShootsDuplicate() {
						SceneMgr.Get().StartCoroutine(loadTexScreenshot());
					}

					private System.Collections.IEnumerator loadTexScreenshot() {
						foreach(Stage.Screenshot screen in Screenshots) {
							if(System.IO.File.Exists(System.IO.Path.Combine(Globals.ScreenshotsPath, screen.IdScreenshot + ".png"))) {
								WWW texToLoad = new WWW("file://" + System.IO.Path.Combine(Globals.ScreenshotsPath, screen.IdScreenshot + ".png"));
								yield return new WaitForEndOfFrame();
								yield return texToLoad;
								screen.texScreen = new Texture2D(Globals.SCREENSHOT_WIDTH, Globals.SCREENSHOT_HEIGHT);
								texToLoad.LoadImageIntoTexture(screen.texScreen);
								yield return new WaitForEndOfFrame();
								yield return new WaitForEndOfFrame();
								texToLoad.Dispose();
							}
						}
					}

					public void loadStage() {
						selObject = Background;
						if(Objects == null) {
							Background.change += changeObject;
							Objects = new Dictionary<int, StageObject>();
							StageObject sObject;
							
							DataTable objects = db.ExecuteQuery("SELECT IdObject, IdObjectType, IdResource FROM Objects WHERE IdStage = " + mIdStage + " AND IdObjectType > 1");
							foreach(DataRow row in objects.Rows) {
								sObject = new StageObject((int)row["IdObject"], (StageObject.Types)row["IdObjectType"], (int)row["IdResource"], this);
								if((int)row["IdObjectType"] == 2)
									SceneCamera = sObject;
								Objects.Add(sObject.IdObject, sObject);
								sObject.change += changeObject;
							}
						}
						Reset(0);
					}
					
					public StageObject newObject(StageObject.Types type, int idResource, Vector3 initPosition, Vector3 initRotation, Vector3 initScale) {
						StageObject stageObject = new StageObject(-1, type, idResource, -1, initPosition, -1, initRotation, -1, initScale, this);
						stageObject.change+=changeObject;
						stageObject.save();
						Objects.Add(stageObject.IdObject, stageObject);
						stageObject.Frame(MiliSeconds, false, false);
						return stageObject;
					}
					
					public Screenshot newScreenshot(int number, bool autoScreenShot) {
						if(number > Screenshots.Count + 1)
							throw new System.Exception("The number is greater than the number of items.");
						Screenshot screen = new Screenshot(-1, number, this);
						if(!autoScreenShot)
							screen.change += changeScreenshot;
						screen.save();
						if(autoScreenShot)
							screen.change += changeScreenshot;
						Screenshots.Add(screen);
						return screen;
					}
			
					private void changeObject(List<CVS.iCVSObject> sender, CVS.CVSEvent eventArgs) {
						if(eventArgs.Modified is StageObject) {
							StageObject stageObject = (StageObject)eventArgs.Modified;
							CVS cvs = new CVS(mVersion, sender, eventArgs.Type, eventArgs.Modified);
							switch(eventArgs.Type) {
							case CVS.CVSTypes.Create:
								mEvents.Add(cvs);
								break;
							case CVS.CVSTypes.Delete:
								stageObject.change-=changeObject;
								Objects.Remove(stageObject.IdObject);
								mEvents.Add(cvs);
								break;
							}
						} else {
							CVS cvs = new CVS(mVersion, sender, eventArgs.Type, eventArgs.Modified);
							mEvents.Add(cvs);
						}
						mVersion++;
						OnChange(sender, eventArgs);
					}
					
					private void changeScreenshot(List<CVS.iCVSObject> sender, CVS.CVSEvent eventArgs) {
						if(eventArgs.Modified is Screenshot) {
							Screenshot screenshot = (Screenshot)eventArgs.Modified;
							CVS cvs = new CVS(mVersion, sender, eventArgs.Type, eventArgs.Modified);
							switch(eventArgs.Type) {
							case CVS.CVSTypes.Renumbered:
								mEvents.Add(cvs);
								int Value;
								int first;
								int last;
								if(screenshot.Number>screenshot.oldNumber) {
									Value=-1;
									first=screenshot.oldNumber;
									last=screenshot.Number;
								} else {
									Value=1;
									first=screenshot.Number;
									last=screenshot.oldNumber;
								}
								renumberScreenshots(screenshot,first,last,Value);
								break;
							case CVS.CVSTypes.Create:
								mEvents.Add(cvs);
								renumberScreenshots(screenshot,screenshot.Number,-1,1);
								break;
							case CVS.CVSTypes.Delete:
								screenshot.change-=changeScreenshot;
								Screenshots.Remove(screenshot);
								mEvents.Add(cvs);
								renumberScreenshots(screenshot,screenshot.Number,-1,-1);
								break;
							}
							mVersion++;
						}
						OnChange(sender, eventArgs);
					}
					
					public bool canReduceToFrames(int frames, bool before) {
						if(minFrames(before) > frames)
							return false;
						return true;
					}

					public void redimFrames(int value, bool before) {
						if(before) {
							if(canReduceToFrames(value, before)) {
								int diff = value - mFrames;
								mFrames = value;
								startBatch();
								save();
								foreach(StageObject.KeyFrame kf in Background.KeyFrames) {
									kf.Frame += diff;
									kf.save();
								}
								foreach(StageObject.BlockTimeline bk in Background.Blocks) {
									bk.StartFrame += diff;
									bk.save();
								}
								foreach(StageObject obj in Objects.Values) {
									foreach(StageObject.KeyFrame kf in obj.KeyFrames) {
										kf.Frame += diff;
										kf.save();
									}
									foreach(StageObject.BlockTimeline bk in obj.Blocks) {
										bk.StartFrame += diff;
										bk.save();
									}
								}
								endBatch();
							} else
								throw new System.Exception("The number of frames is too little.");
						} else {
							if(canReduceToFrames(value, before)) {
								mFrames = value;
								save();
							} else
								throw new System.Exception("The number of frames is too little.");
						}
					}

					public void startBatch() {
						if(mBatchStarted)
							throw new System.Exception("There is a batch.");
						CVS cvs = new CVS(0, null, CVS.CVSTypes.StartBatch, null);
						mEvents.Add(cvs);
						mBatchStarted = true;
					}

					public void endBatch() {
						if(!mBatchStarted)
							throw new System.Exception("There is a no batch.");
						CVS cvs = new CVS(0, null, CVS.CVSTypes.EndBatch, null);
						mEvents.Add(cvs);
						mBatchStarted = false;
					}

					public void delete() {
						db.ExecuteNonQuery("UPDATE Stages SET CVSDel = " + mParent.Version + " WHERE IdStage = " + mIdStage + " AND CVSDel IS NULL");
						CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Delete, this);
						OnChange(evt);
					}
					
					public void save(bool loadBackground = true) {
						if(mIdStage == -1) {
							db.ExecuteNonQuery("INSERT INTO Stages (IdScene, CVSNew) VALUES (" + mParent.IdScene + "," + mParent.Version + ")");
							mIdStage = (int)db.ExecuteQuery("SELECT MAX(IdStage) as ID FROM Stages")[0]["ID"];
							db.ExecuteNonQuery("UPDATE StagesInfo SET Number = " + mNumber + ", Title = '" + Title.Replace("'", "''").Trim() + "', Information = '" + Information.Replace("'", "''").Trim() + "', Frames = " + mFrames + " WHERE IdStage = " + mIdStage + " AND CVSNew = -1");
							DataRow row = db.ExecuteQuery("SELECT IdObject FROM Objects WHERE IdStage = " + mIdStage + " AND IdObjectType  = " + (int)Stage.StageObject.Types.Background)[0];
							Background = new StageObject((int)row["IdObject"], Stage.StageObject.Types.Background, -1, this, loadBackground);
							CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Create, this);
							OnChange(evt);
							if(mFrames != 0) {
								evt = new CVS.CVSEvent(CVS.CVSTypes.UpdateDuration, this);
								OnChange(evt);
							}
						} else {
							//Si mOldFrames == 0 solo actualizar y no lanzar el evento Info, la primera que se asigna no se puede desahacer.
							if(mOldInformation == Information && mOldTitle == Title && mOldFrames == mFrames)
								return;
							CVS.CVSEvent evt;
							if(mOldFrames == 0) {
								db.ExecuteNonQuery("UPDATE StagesInfo SET Frames = " + mFrames + " WHERE IdStage = " + mIdStage);// + " AND CVSDel IS NULL");
								mOldFrames = mFrames;
								evt = new CVS.CVSEvent(CVS.CVSTypes.UpdateDuration, this);
								OnChange(evt);
							}

							//Lanzamos solo el evento para que actualice la duración.
							//Apuntamos en la lista de deshacer de la stage, solo se puede deshacer desde el editor el cambio de número de frames.
							//Trabajamos con la versión del padre porque la clase stage normalmente se deshace desde el editor de la escena.
							//Usamos la versión en negativo a efectos informativos.
							if(mOldFrames != mFrames) {
								db.ExecuteNonQuery("UPDATE StagesInfo SET CVSDel = " + mParent.Version * -1 + " WHERE IdStage = " + mIdStage + " AND CVSDel IS NULL");
								db.ExecuteNonQuery("INSERT INTO StagesInfo (IdStage, CVSNew, Number, Title, Information, Frames) VALUES (" + mIdStage + ", " + mParent.Version * -1 + ", " + mNumber + ", '" + Title.Replace("'", "''").Trim() + "', '" + Information.Replace("'", "''").Trim() + "', " + mFrames + ")");
								CVS cvs = new CVS(mParent.Version * -1, null, CVS.CVSTypes.Info, this);
								mEvents.Add(cvs);
								evt = new CVS.CVSEvent(CVS.CVSTypes.UpdateDuration, this);
								OnChange(evt);
								mOldFrames = mFrames;
							}
							
							if(mOldInformation == Information && mOldTitle == Title)
								return;
							db.ExecuteNonQuery("UPDATE StagesInfo SET CVSDel = " + mParent.Version + " WHERE IdStage = " + mIdStage + " AND CVSDel IS NULL");
							db.ExecuteNonQuery("INSERT INTO StagesInfo (IdStage, CVSNew, Number, Title, Information, Frames) VALUES (" + mIdStage + ", " + mParent.Version + ", " + mNumber + ", '" + Title.Replace("'", "''").Trim() + "', '" + Information.Replace("'", "''").Trim() + "', " + mFrames + ")");
							evt = new CVS.CVSEvent(CVS.CVSTypes.Info, this);
							OnChange(evt);
							mOldTitle = Title;
							mOldInformation = Information;
						}
					}
					
					public void Renumber(int number) {
						if(number > mParent.Stages.Count)
							throw new System.Exception("The number is greater than the number of items.");
						if(mNumber != number) {
							mOldNumber = mNumber;
							mNumber = number;
							db.ExecuteNonQuery("UPDATE StagesInfo SET CVSDel = " + mParent.Version + " WHERE IdStage = " + mIdStage + " AND CVSDel IS NULL");
							db.ExecuteNonQuery("INSERT INTO StagesInfo (IdStage, CVSNew, Number, Title, Information, Frames) VALUES (" + mIdStage + ", " + mParent.Version + ", " + mNumber + ", '" + mOldTitle.Replace("'", "''").Trim() + "', '" + mOldInformation.Replace("'", "''").Trim() + "', " + mOldFrames + ")");
							CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Renumbered, this);
							OnChange(evt);
						}
					}
					
					public void Frame(float miliSeconds, bool play, bool omitCameraRot=false, bool player = false) {
						miliSeconds += 0.00001f;
						if(MiliSeconds != miliSeconds) {
							MiliSeconds = miliSeconds;
							Background.Frame(miliSeconds, play, omitCameraRot, player);
							foreach(StageObject StageObj in Objects.Values) {
								StageObj.Frame(miliSeconds, play, omitCameraRot, player);
							}
						}
					}
					
					public void Reset(float miliSeconds) {
						MiliSeconds = -1;
						Background.Reset(miliSeconds);
						foreach(StageObject StageObj in Objects.Values) {
							StageObj.Reset(miliSeconds);
						}
					}
					
					public void Stop(float miliSeconds, bool Play = false) {
						Background.Stop(miliSeconds, Play);
						foreach(StageObject StageObj in Objects.Values) {
							StageObj.Stop(miliSeconds, Play);
						}
					}
	
					private void renumberScreenshots(Screenshot screenshotCompare, int firstNumber, int lastNumber, int Value) {
						if(lastNumber == -1)
							lastNumber = 1000000;
					
						foreach(Screenshot screenshot in Screenshots) {
							if(screenshot.Number >= firstNumber && screenshot.Number <= lastNumber && screenshot != screenshotCompare) {
								screenshot.mNumber += Value;
								db.ExecuteNonQuery("UPDATE ScreenshotsInfo SET Number = " + screenshot.Number + " WHERE IdScreenshot = " + screenshot.IdScreenshot + " AND CVSDel IS NULL");
							}
						}
						Screenshots.Sort();
					}
	
					internal override bool undoLocal(CVS cvs) {
						CVS.CVSEvent evt;
						switch(cvs.Type) {
						case CVS.CVSTypes.Create:
							db.ExecuteNonQuery("DELETE FROM Stages WHERE IdStage = " + mIdStage + " AND CVSNew = " + cvs.Version);
							evt = new CVS.CVSEvent(CVS.CVSTypes.UpdateDuration, this);
							OnChange(evt);
							return true;
						case CVS.CVSTypes.Delete:
							db.ExecuteNonQuery("UPDATE Stages SET CVSDel = NULL WHERE IdStage = " + mIdStage + " AND CVSDel = " + cvs.Version);
							evt = new CVS.CVSEvent(CVS.CVSTypes.UpdateDuration, this);
							OnChange(evt);
							return true;
						case CVS.CVSTypes.Renumbered:
							int sceneNumber = (int)db.ExecuteQuery("SELECT Number FROM StagesInfo WHERE IdStage = " + mIdStage + " AND CVSDel = " + cvs.Version)[0]["Number"];
							db.ExecuteNonQuery("DELETE FROM StagesInfo WHERE IdStage = " + mIdStage + " AND CVSDel IS NULL");
							db.ExecuteNonQuery("UPDATE StagesInfo SET CVSDel = NULL WHERE IdStage = " + mIdStage + " AND CVSDel = " + cvs.Version);
							mOldNumber = mNumber;
							mNumber = sceneNumber;
							return true;
						case CVS.CVSTypes.Info:
							DataRow row = db.ExecuteQuery("SELECT Title, Information FROM StagesInfo WHERE IdStage = " + mIdStage + " AND CVSDel = " + cvs.Version)[0];
							Title = (string)row["Title"];
							mOldTitle = (string)row["Title"];
							Information = (string)row["Information"];
							mOldInformation = (string)row["Information"];
							db.ExecuteNonQuery("DELETE FROM StagesInfo WHERE IdStage = " + mIdStage + " AND CVSDel IS NULL");
							db.ExecuteNonQuery("UPDATE StagesInfo SET CVSDel = NULL WHERE IdStage = " + mIdStage + " AND CVSDel = " + cvs.Version);
							return true;
						}
						return false;
					}
				
					public void undo(undoDelegate undoDel) {
						if(mEvents.Count == 0)
							return;
						mUndoDelegate = undoDel;
						CVS cvs = mEvents[mEvents.Count - 1];
						if(cvs.Type == CVS.CVSTypes.EndBatch) {
							mEvents.Remove(cvs);
							undoRecursive();
						} else {
							undo2(cvs);
						}
					}

					private void undo2(CVS cvs) {
						if(cvs.Modified is Stage) {
							DataRow row = db.ExecuteQuery("SELECT Frames FROM StagesInfo WHERE IdStage = " + mIdStage + " AND CVSDel = " + cvs.Version)[0];
							mFrames = (int)row["Frames"];
							mOldFrames = (int)row["Frames"];
							db.ExecuteNonQuery("DELETE FROM StagesInfo WHERE IdStage = " + mIdStage + " AND CVSDel IS NULL");
							db.ExecuteNonQuery("UPDATE StagesInfo SET CVSDel = NULL WHERE IdStage = " + mIdStage + " AND CVSDel = " + cvs.Version);
							CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.UpdateDuration, this);
							OnChange(evt);
							if(mUndoDelegate != null)
								mUndoDelegate(cvs.Type, this, null);
							mEvents.Remove(cvs);
						} else if(cvs.Modified is Screenshot) {
							undoScreenshot();
						} else if(cvs.Modified is StageObject) {
							undoObject();
						} else {
							CVS.iCVSObject CVSObject = (CVS.iCVSObject)cvs.Sender[cvs.Sender.Count - 1];
							cvs.Sender.Remove(CVSObject);
							if(CVSObject.undoLocal(cvs))
								mEvents.Remove(cvs);
						}
					}

					private void undoRecursive() {
						if(mEvents.Count == 0)
							return;
						CVS cvs = mEvents[mEvents.Count - 1];
						if(cvs.Type == CVS.CVSTypes.StartBatch) {
							mEvents.Remove(cvs);
						} else {
							undo2(cvs);
							undoRecursive();
						}
					}
					
					private void undoScreenshot() {
						Screenshot screenshot;
						CVS cvs = mEvents[mEvents.Count - 1];
						switch(cvs.Type) {
						case CVS.CVSTypes.Create:
							BRB.Utils.Message.Show(0, Texts.WARNING, Texts.STAGE_UNDO_CREATE_SCREENSHOT, BRB.Utils.Message.Type.YesNo, Texts.YES, Texts.NO, undoMessageCreate);
							return;
						case CVS.CVSTypes.Delete:
							screenshot = (Screenshot)cvs.Modified;
							if(screenshot.undoLocal(cvs)) {
								screenshot.change += changeScreenshot;
								Screenshots.Add(screenshot);
								renumberScreenshots(screenshot, screenshot.Number, -1, 1);
								
								mEvents.Remove(cvs);
								if(mUndoDelegate != null)
									mUndoDelegate(CVS.CVSTypes.Delete, screenshot, null);
								return;
							}
							break;
						case CVS.CVSTypes.Renumbered:
							screenshot = (Screenshot)cvs.Modified;
							if(screenshot.undoLocal(cvs)) {
								int Value;
								int first;
								int last;
								if(screenshot.Number > screenshot.oldNumber) {
									Value = -1;
									first = screenshot.oldNumber;
									last = screenshot.Number;
								} else {
									Value = 1;
									first = screenshot.Number;
									last = screenshot.oldNumber;
								}
								renumberScreenshots(screenshot, first, last, Value);

								mEvents.Remove(cvs);
								if(mUndoDelegate != null)
									mUndoDelegate(CVS.CVSTypes.Renumbered, screenshot, null);
								return;
							}
							break;
						}
						if(mUndoDelegate != null)
							mUndoDelegate(CVS.CVSTypes.Nothing, null, null);
					}
					
					private void undoObject() {
						StageObject stageObject;
						CVS cvs = mEvents[mEvents.Count - 1];
						switch(cvs.Type) {
						case CVS.CVSTypes.Create:
							//undoMessageCreate(BRB.Utils.Message.ButtonClicked.Yes,1);
							BRB.Utils.Message.Show(1, Texts.WARNING, Texts.STAGE_UNDO_CREATE_OBJECT, BRB.Utils.Message.Type.YesNo, Texts.YES, Texts.NO, undoMessageCreate);
							return;
						case CVS.CVSTypes.Delete:
							stageObject = (StageObject)cvs.Modified;
							if(stageObject.undoLocal(cvs)) {
								stageObject.change += changeObject;
								Objects.Add(stageObject.IdObject, stageObject);
								
								mEvents.Remove(cvs);
								if(mUndoDelegate != null)
									mUndoDelegate(CVS.CVSTypes.Delete, stageObject, null);
								stageObject.Reset(MiliSeconds);
								stageObject.Stop(MiliSeconds, false);
								stageObject.Frame(MiliSeconds, false, false);
								return;
							}
							break;
						}
						if(mUndoDelegate != null)
							mUndoDelegate(CVS.CVSTypes.Nothing, null, null);
					}
					
					private void undoMessageCreate(BRB.Utils.Message.ButtonClicked buttonClicked, int Identifier) {
						if(buttonClicked == BRB.Utils.Message.ButtonClicked.Yes) {
							CVS cvs;
							switch(Identifier) {
							case 0: //Screenshot
								cvs = mEvents[mEvents.Count - 1];
								Screenshot screenshot = (Screenshot)cvs.Modified;
								if(screenshot.undoLocal(cvs)) {
									screenshot.change -= changeScreenshot;
									Screenshots.Remove(screenshot);
									renumberScreenshots(screenshot, screenshot.Number, -1, -1);
									
									mEvents.Remove(cvs);
									if(mUndoDelegate != null)
										mUndoDelegate(CVS.CVSTypes.Create, screenshot, null);
									return;
								}
								break;
							case 1: //StageObject
								cvs = mEvents[mEvents.Count - 1];
								StageObject stageObject = (StageObject)cvs.Modified;
								if(stageObject.undoLocal(cvs)) {
									stageObject.change -= changeObject;
									Objects.Remove(stageObject.IdObject);
									
									mEvents.Remove(cvs);
									if(mUndoDelegate != null)
										mUndoDelegate(CVS.CVSTypes.Create, stageObject, null);
									return;
								}
								break;
							}
						}
						if(mUndoDelegate != null)
							mUndoDelegate(CVS.CVSTypes.Nothing, null, null);
					}

					public int undoCounts() {
						return mEvents.Count;
					}
					
					public void saveAudioClips(string path, System.IO.StreamWriter log, Export_Main export) {
						byte[] emptySamplesBytes = null;
						if(mFrames * Globals.MILISPERFRAME < 15)
							emptySamplesBytes = new byte[mFrames * Globals.NUMCHANNELS * Globals.OUTPUTRATEPERFRAME * sizeof(short)];
						saveAudioClips(mFrames * Globals.OUTPUTRATEPERFRAME, emptySamplesBytes, 0, path, log, export);
					}

					internal void saveAudioClips(int samples, byte[] emptySamplesBytes, int framesOffset, string path, System.IO.StreamWriter log, Export_Main export) {
						//log.WriteLine("Exportando escena " + mNumber + " de la secuencia " + sceneNumber + " del episodio " + episodeNumber + ".");
						QueueManager.add(new QueueManager.QueueManagerAction("Export", () => log.WriteLine("Exportando audio escena " + mNumber + " de la secuencia " + sceneNumber + " del episodio " + episodeNumber + "."), "Stage.Log(Exportando)"), QueueManager.Priorities.High);
						Dictionary<string, int> objects = new Dictionary<string, int>();
						//QueueManager.add(() => Background.saveAudioClips(objects, samples, emptySamplesBytes, framesOffset, path, log));
						Background.saveAudioClips(objects, samples, emptySamplesBytes, framesOffset, path, log, export);
						foreach(StageObject obj in Objects.Values) {
							//QueueManager.add(() => obj.saveAudioClips(objects, samples, emptySamplesBytes, framesOffset, path, log));
							obj.saveAudioClips(objects, samples, emptySamplesBytes, framesOffset, path, log, export);
						}
					}

					public int CompareTo(Stage other) {		
						if(this.Number < other.Number) return -1;
						else if(this.Number > other.Number) return 1;
						else return 0;
					}

					public class Screenshot : CVS.iCVSObject, System.IComparable<Screenshot> {
						private int mIdScreenshot;
						private Stage mParent;
						internal int mNumber;
						private int mOldNumber;

						//internal event changeEvent change;
						public Texture2D texScreen;
						private bool mScreenInProgress;
						
						public int IdScreenshot {
							get { return mIdScreenshot; }
						}
						public int IdStage {
							get { return mParent.IdStage; }
						}
						public int Number {
							get { return mNumber; }
						}
						public int oldNumber {
							get { return mOldNumber; }
						}

						internal Screenshot(int idScreenshot, int Number, Stage parent) {
							mIdScreenshot=idScreenshot;
							mParent=parent;
							mNumber=Number;
							mScreenInProgress=false;
						}
						
						public void save() {
							if(mIdScreenshot == -1) {
								db.ExecuteNonQuery("INSERT INTO Screenshots (IdStage, CVSNew) VALUES (" + mParent.IdStage + ", " + mParent.Version + ")");
								mIdScreenshot = (int)db.ExecuteQuery("SELECT MAX(IdScreenshot) as ID FROM Screenshots")[0]["ID"];
								db.ExecuteNonQuery("UPDATE ScreenshotsInfo SET Number = " + mNumber + " WHERE IdScreenshot = " + mIdScreenshot + " AND CVSNew = -1");
								CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Create, this);
								OnChange(evt);
								updateTexScreenshot();
							}
						}
						
						public void delete() {
							db.ExecuteNonQuery("UPDATE Screenshots SET CVSDel = " + mParent.Version + " WHERE IdScreenshot = " + mIdScreenshot + " AND CVSDel IS NULL");
							CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Delete, this);
							OnChange(evt);
						}

						public void updateTexScreenshot() {
							SceneMgr.Get().StartCoroutine(ScreenShotToTex(100, 0, Screen.width - 200, Screen.height - 120));
						}
						
						private System.Collections.IEnumerator ScreenShotToTex(int iX, int iY, int iWidth, int iHeight) {
							if(!mScreenInProgress) {
								mScreenInProgress = true;
								int oldAA = QualitySettings.antiAliasing;
								QualitySettings.antiAliasing = 0;
								yield return new WaitForEndOfFrame();
								yield return new WaitForEndOfFrame();
								yield return new WaitForEndOfFrame();
								
								iY = Screen.height - iHeight - iY;
								
								Texture2D tex = new Texture2D(iWidth, iHeight, TextureFormat.RGB24, true);
								tex.ReadPixels(new Rect(iX, iY, iWidth, iHeight), 0, 0);
								tex.Apply();
								QualitySettings.antiAliasing = oldAA;
								
								yield return new WaitForEndOfFrame();
								Texture2D newTex = new Texture2D(iWidth / 4, iHeight / 4);// ScaleTexture(tex,81,81);
								newTex.SetPixels(tex.GetPixels(2));
								newTex.Apply();
								
								yield return new WaitForEndOfFrame();
								byte[] bytes = newTex.EncodeToPNG();
								string fileName = System.IO.Path.Combine(BRBRec.Globals.ScreenshotsPath, mIdScreenshot + ".png");		
								System.IO.File.WriteAllBytes(fileName, bytes);
								texScreen = newTex;
								mScreenInProgress = false;
							}
						}
						
						public void Renumber(int number) {
							if(number > mParent.Screenshots.Count)
								throw new System.Exception("The number is greater than the number of items.");
							if(mNumber != number) {
								mOldNumber = mNumber;
								mNumber = number;
								db.ExecuteNonQuery("UPDATE ScreenshotsInfo SET CVSDel = " + mParent.Version + " WHERE IdScreenshot = " + mIdScreenshot + " AND CVSDel IS NULL");
								db.ExecuteNonQuery("INSERT INTO ScreenshotsInfo (IdScreenshot, CVSNew, Number) VALUES (" + mIdScreenshot + ", " + mParent.Version + ", " + mNumber + ")");
								CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Renumbered, this);
								OnChange(evt);
							}
						}
						
						internal override bool undoLocal(CVS cvs) {
							switch(cvs.Type) {
							case CVS.CVSTypes.Create:
								db.ExecuteNonQuery("DELETE FROM Screenshots WHERE IdScreenshot = " + mIdScreenshot + " AND CVSNew = " + cvs.Version);
								DeleteScreenShot();
								return true;
							case CVS.CVSTypes.Delete:
								db.ExecuteNonQuery("UPDATE Screenshots SET CVSDel = NULL WHERE IdScreenshot = " + mIdScreenshot + " AND CVSDel = " + cvs.Version);
								return true;
							case CVS.CVSTypes.Renumbered:
								int sceneNumber = (int)db.ExecuteQuery("SELECT Number FROM ScreenshotsInfo WHERE IdScreenshot = " + mIdScreenshot + " AND CVSDel = " + cvs.Version)[0]["Number"];
								db.ExecuteNonQuery("DELETE FROM ScreenshotsInfo WHERE IdScreenshot = " + mIdScreenshot + " AND CVSDel IS NULL");
								db.ExecuteNonQuery("UPDATE ScreenshotsInfo SET CVSDel = NULL WHERE IdScreenshot = " + mIdScreenshot + " AND CVSDel = " + cvs.Version);
								mOldNumber = mNumber;
								mNumber = sceneNumber;
								return true;
							}
							return false;
						}
						
						private void DeleteScreenShot() {
							if(System.IO.File.Exists(System.IO.Path.Combine(BRBRec.Globals.ScreenshotsPath, mIdScreenshot + ".png")))
								System.IO.File.Delete(System.IO.Path.Combine(BRBRec.Globals.ScreenshotsPath, mIdScreenshot + ".png"));
						}
						
						public int CompareTo(Screenshot other) {		
							if(this.Number < other.Number) return -1;
							else if(this.Number > other.Number) return 1;
							else return 0;
						}
					}
					
					public class StageObject : CVS.iCVSObject {
						public const int EMPTY_RESOURCE = -1;
						public enum Types {
							Background = 1,
							Camera = 2,
							Character = 3,
							Prop = 4,
							FX = 5,
						}
						
						private int mIdObject;
						private Types mType;
						private int mIdResource;
						private Stage mParent;
						//internal event changeEvent change;
						private GameObject mGameObject;
						public KeyFrame initPos;
						public KeyFrame initRot;
						public KeyFrame initSca;
						
						public int IdObject {
							get { return mIdObject; }
						}
						public int IdStage {
							get { return mParent.IdStage; }
						}
						public Types Type {
							get { return mType; }
						}
						public int IdResource {
							get { return mIdResource; }
							set {
								if(mType != Types.Background)
									throw new System.Exception("You only can modify the resourse of a background object.");
								else if(mIdResource != EMPTY_RESOURCE)
									throw new System.Exception("You only can modify the resourse of a background object one time.");
								else if(mIdResource != value) {
									mIdResource = value;
									db.ExecuteNonQuery("UPDATE Objects SET IdResource = " + mIdResource + " WHERE IdObject = " + mIdObject);
								}
							}
						}
						public string Name {
							get { return resource().Name; }
						}
						public int Version {
							get { return mParent.Version; }
						}

						private Texture mMicro;
						private string mStrMicro;
						private Texture mMini;
						private string mStrMini;
						
						internal undoDelegate mUndoDelegate {
							get { return mParent.mUndoDelegate; }
						}
						
						private List<CVS> mEvents;
						
						private CVS mCVS;

						private bool mIdlePlaying;
						
						private BezierPath mBPPosition;
						private BezierPath mBPRotation;
						private BezierPath mBPScale;
						private BezierPath[] mBezierPath;
						
						#region List
						public List<KeyFrame> KFPosition;
						public List<KeyFrame> KFRotation;
						public List<KeyFrame> KFScale;
						public List<KeyFrame> KeyFrames;
						
						public List<KeyFrame>[] mKeyFrames;
						
						public List<BlockTimeline> BVisibility;
						public List<BlockTimeline> BAnimation;
						public List<BlockTimeline> BExpression;
						public List<BlockTimeline> BSound;
						public List<BlockTimeline> BVoice;
						public List<BlockTimeline> BMusic;
						public List<BlockTimeline> BProp1;
						public List<BlockTimeline> BProp2;
						public List<BlockTimeline> BProp3;
						public List<BlockTimeline> BSprite2D1;
						public List<BlockTimeline> BSprite2D2;
						public List<BlockTimeline> Blocks;
						
						private List<BlockTimeline>[] mBlocks;
						private List<BlockTimeline> mBlocksPerfomed;
						#endregion
						
						internal StageObject(int idObject, Types type, int idResource, Stage parent, bool LoadObject = true) {
							mIdObject=idObject;
							mType=type;
							mIdResource=idResource;
							mParent=parent;
							mStrMicro="";
							mStrMini="";
							mIdlePlaying=false;
							if(LoadObject)
								loadObject();
						}
						
						internal StageObject(int idObject, Types type, int idResource,int idKFInitPosition, Vector3 position,int idKFInitRotation, Vector3 rotation,int idKFInitScale, Vector3 scale, Stage parent) : this(idObject,type,idResource,parent) {
							initPos=new KeyFrame(idKFInitPosition, KeyFrame.Types.Position,-1,position, false, this);
							initRot=new KeyFrame(idKFInitRotation, KeyFrame.Types.Rotation,-1,rotation, false, this);
							initSca=new KeyFrame(idKFInitScale, KeyFrame.Types.Scale,-1,scale, false, this);
						}
						
						internal void loadObject() {
							if(mKeyFrames==null) {
								KFPosition=new List<KeyFrame>();
								KFRotation=new List<KeyFrame>();
								KFScale=new List<KeyFrame>();
								KeyFrames=new List<KeyFrame>();
								
								mBPPosition=new BezierPath();
								mBPRotation=new BezierPath();
								mBPScale=new BezierPath();
								mBezierPath = new BezierPath[3];
								mBezierPath[0]=mBPPosition;
								mBezierPath[1]=mBPRotation;
								mBezierPath[2]=mBPScale;
								
								mKeyFrames=new List<KeyFrame>[3];
								mKeyFrames[0]=KFPosition;
								mKeyFrames[1]=KFRotation;
								mKeyFrames[2]=KFScale;
								
								DataTable keyFrames = db.ExecuteQuery("SELECT KeyFrames.IdKeyFrame, IdKeyFrameType, Frame, X, Y, Z, Control FROM KeyFrames INNER JOIN KeyFramesInfo ON KeyFrames.IdKeyFrame = KeyFramesInfo.IdKeyFrame WHERE IdObject = " + mIdObject);
								KeyFrame keyFrame;
								foreach(DataRow row in keyFrames.Rows) {
									keyFrame = new KeyFrame((int)row["IdKeyFrame"],(KeyFrame.Types)row["IdKeyFrameType"],(int)row["Frame"],new Vector3(System.Convert.ToSingle(row["X"]),System.Convert.ToSingle(row["Y"]),System.Convert.ToSingle(row["Z"])),System.Convert.ToBoolean(row["Control"]),this);
									if((int)row["Frame"]!=-1) {
										mKeyFrames[(int)row["IdKeyFrameType"]-1].Add(keyFrame);
										KeyFrames.Add(keyFrame);
									} else {
										switch((KeyFrame.Types)row["IdKeyFrameType"]) {
										case KeyFrame.Types.Position:
											initPos=keyFrame;
											break;
										case KeyFrame.Types.Rotation:
											initRot=keyFrame;
											break;
										case KeyFrame.Types.Scale:
											initSca=keyFrame;
											break;
										}
									}
									keyFrame.change+=changeKF;
								}
								
								KeyFrames.Sort();
								for(int i=0; i<mKeyFrames.Length; ++i) {
									mKeyFrames[i].Sort();
								}
								
								interpolateKF (KeyFrame.Types.Position);
								interpolateKF (KeyFrame.Types.Rotation);
								interpolateKF (KeyFrame.Types.Scale);
								
								//////////////////////////////////////								
								BVisibility=new List<BlockTimeline>();
								BAnimation=new List<BlockTimeline>();
								BExpression=new List<BlockTimeline>();
								BSound=new List<BlockTimeline>();
								BVoice=new List<BlockTimeline>();
								BMusic=new List<BlockTimeline>();
								BProp1=new List<BlockTimeline>();
								BProp2=new List<BlockTimeline>();
								BProp3=new List<BlockTimeline>();
								BSprite2D1=new List<BlockTimeline>();
								BSprite2D2=new List<BlockTimeline>();
								Blocks=new List<BlockTimeline>();
								mBlocksPerfomed=new List<BlockTimeline>();
								
								mBlocks=new List<BlockTimeline>[11];
								mBlocks[0]=BVisibility;
								mBlocks[1]=BAnimation;
								mBlocks[2]=BExpression;
								mBlocks[3]=BSound;
								mBlocks[4]=BVoice;
								mBlocks[5]=BMusic;
								mBlocks[6]=BProp1;
								mBlocks[7]=BProp2;
								mBlocks[8]=BProp3;
								mBlocks[9]=BSprite2D1;
								mBlocks[10]=BSprite2D2;

								DataTable blocks = db.ExecuteQuery("SELECT BlocksTimeline.IdBlockTimeline, IdBlockType, IdResource, StartFrame, Frames, Dummy, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, ScaleX, ScaleY, ScaleZ, Control FROM BlocksTimeline INNER JOIN BlocksTimelineInfo ON BlocksTimeline.IdBlockTimeline = BlocksTimelineInfo.IdBlockTimeline LEFT OUTER JOIN BlocksTimelineProp ON BlocksTimeline.IdBlockTimeline = BlocksTimelineProp.IdBlockTimeline WHERE IdObject = " + mIdObject);
								BlockTimeline block;
								foreach(DataRow row in blocks.Rows) {
									if((BlockTimeline.Types)row["IdBlockType"] == BlockTimeline.Types.Prop1 || (BlockTimeline.Types)row["IdBlockType"] == BlockTimeline.Types.Prop2 || (BlockTimeline.Types)row["IdBlockType"] == BlockTimeline.Types.Prop3)
										block = new BlockTimelineProp((int)row["IdBlockTimeline"], (BlockTimeline.Types)row["IdBlockType"], (int)row["IdResource"], (int)row["StartFrame"], (int)row["Frames"], (int)row["Control"], this, (string)row["Dummy"], new Vector3(System.Convert.ToSingle(row["PositionX"]), System.Convert.ToSingle(row["PositionY"]), System.Convert.ToSingle(row["PositionZ"])), new Vector3(System.Convert.ToSingle(row["RotationX"]), System.Convert.ToSingle(row["RotationY"]), System.Convert.ToSingle(row["RotationZ"])), new Vector3(System.Convert.ToSingle(row["ScaleX"]), System.Convert.ToSingle(row["ScaleY"]), System.Convert.ToSingle(row["ScaleZ"])));
									else
										block = new BlockTimeline((int)row["IdBlockTimeline"],(BlockTimeline.Types)row["IdBlockType"],(int)row["IdResource"],(int)row["StartFrame"],(int)row["Frames"],(int)row["Control"],this);
									mBlocks[(int)row["IdBlockType"]-1].Add(block);
									Blocks.Add(block);
									block.change+=changeBlock;
								}
								
								Blocks.Sort();
								for(int i=0; i<mBlocks.Length; ++i) {
									mBlocks[i].Sort();
								}
							}
						}
						
						public void interpolateKF(KeyFrame.Types type) {
							List<Vector3> lst=new List<Vector3>();
							foreach(KeyFrame kf in mKeyFrames[(int)type -1]) {
								lst.Add(kf.Vector);
								if(!kf.Control)
									lst.Add(kf.Vector);
							}
							mBezierPath[(int)type -1].Interpolate(lst,.25f);
						}
						
						public void delete() {
							if(mType == Types.Camera || mType == Types.Background)
								throw new System.Exception("You can't delete a camera or background object.");
							db.ExecuteNonQuery("UPDATE Objects SET CVSDel = " + mParent.Version + " WHERE IdObject = " + mIdObject + " AND CVSDel IS NULL");
							destroyInstance();
							CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Delete, this);
							OnChange(evt);
						}
						
						public void destroyInstance() {
							if(mType == Types.Camera || mType == Types.Background)
								throw new System.Exception("You can't destroy a camera or background object.");
							if(Data.selStage.selObject == this) {
								Data.selStage.selObject = Data.selStage.Background;
							}
							Scene_Main.getEditorInterface.DestroyedElement(getInstance("Scene"));
							MonoBehaviour.Destroy(getInstance("Scene"));
							mGameObject = null;
						}
						
						public void destroyInstancePlayer() {
							if(mGameObject != null) {
								MonoBehaviour.Destroy(mGameObject);
								mGameObject = null;
							}
							foreach(BlockTimelineProp bk in BProp1) {
								bk.destroyInstancePlayer();
							}
							foreach(BlockTimelineProp bk in BProp2) {
								bk.destroyInstancePlayer();
							}
							foreach(BlockTimelineProp bk in BProp3) {
								bk.destroyInstancePlayer();
							}
						}
						
						public void save() {
							if(mIdObject==-1) {
								if(mType == Types.Camera || mType == Types.Background)
									throw new System.Exception("You can't create a camera or background object.");
								if(mIdResource==-1)
									throw new System.Exception("You can't create an object without resource.");
								
								db.ExecuteNonQuery("INSERT INTO Objects (IdStage, IdObjectType, CVSNew, IdResource) VALUES (" + mParent.IdStage + ", " + (int)mType + ", " + mParent.Version + ", " + mIdResource + ")");
								mIdObject=(int)db.ExecuteQuery("SELECT MAX(IdObject) as ID FROM Objects")[0]["ID"];
								
								initPos.save();
								initPos.change+=changeKF; //Invertimos el orden para no recibir el evento.
								initRot.save();
								initRot.change+=changeKF; //Invertimos el orden para no recibir el evento.
								initSca.save();
								initSca.change+=changeKF; //Invertimos el orden para no recibir el evento.
								
								//Crear bloque de visibilidad.
								BlockTimeline block = new BlockTimeline(-1, BlockTimeline.Types.Visibility, -1, 0, mParent.DurationInFrames, -1, this);
								block.save();
								block.change+=changeBlock; //Invertimos el orden para no recibir el evento.
								BVisibility.Add(block);
								Blocks.Add(block);
								
								CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Create, this);
								OnChange (evt);
							} else {
								throw new System.Exception("You can't modify objects.");
							}
						}
						
						public KeyFrame newKeyFrame(KeyFrame.Types type, int frame, Vector3 vector, bool control) {
							KeyFrame keyFrame = new KeyFrame(-1, type, frame, vector, control, this);
							keyFrame.change+=changeKF;
							if(frame!=-1) {
								mKeyFrames[(int)type-1].Add(keyFrame);
								mKeyFrames[(int)type-1].Sort();
								KeyFrames.Add(keyFrame);
								KeyFrames.Sort();
							}
							keyFrame.save();
							return keyFrame;
						}
						
						public BlockTimeline newBlock(BlockTimeline.Types type, int idResource, int startFrame, int frames, bool control, int controlValue) {
							if(type==BlockTimeline.Types.Prop1 || type==BlockTimeline.Types.Prop2 || type==BlockTimeline.Types.Prop3)
								throw new System.Exception("To create a new prop block, you must use newBlockProp.");
							if(controlValue <= 0)
								throw new System.Exception("Control value must be a value greater than 0.");
							if(!control)
								controlValue *= -1;
							BlockTimeline block = new BlockTimeline(-1, type, idResource, startFrame, frames, controlValue, this);
							block.change+=changeBlock;
							mBlocks[(int)type-1].Add(block);
							mBlocks[(int)type-1].Sort();
							Blocks.Add(block);
							Blocks.Sort();
							block.save();
							return block;
						}

						public BlockTimelineProp newBlockProp(BlockTimeline.Types type, int idResource, int startFrame, int frames, bool control, int controlValue, string dummy, Vector3 position, Vector3 rotation, Vector3 scale) {
							if(type!=BlockTimeline.Types.Prop1 && type!=BlockTimeline.Types.Prop2 && type!=BlockTimeline.Types.Prop3)
								throw new System.Exception("To create a new block, you must use newBlock.");
							if(controlValue <= 0)
								throw new System.Exception("Control value must be a value greater than 0.");
							if(!control)
								controlValue *= -1;
							BlockTimelineProp block = new BlockTimelineProp(-1, type, idResource, startFrame, frames, controlValue, this, dummy, position, rotation, scale);
							block.change+=changeBlock;
							mBlocks[(int)type-1].Add(block);
							mBlocks[(int)type-1].Sort();
							Blocks.Add(block);
							Blocks.Sort();
							block.save();
							return block;
						}

						private void changeKF(List<CVS.iCVSObject> sender, CVS.CVSEvent eventArgs) {
							if(eventArgs.Modified is KeyFrame) {
								KeyFrame keyframe = (KeyFrame)eventArgs.Modified;
								switch(eventArgs.Type) {
								case CVS.CVSTypes.Delete:
									keyframe.change-=changeKF;
									KeyFrames.Remove(keyframe);
									mKeyFrames[(int)keyframe.Type-1].Remove(keyframe);
									break;
								case CVS.CVSTypes.Info:
									mKeyFrames[(int)keyframe.Type-1].Sort();
									break;
								}
								interpolateKF(keyframe.Type);
								Frame(mParent.MiliSeconds,false);
							}
							OnChange(sender, eventArgs);
						}
						
						private void changeBlock(List<CVS.iCVSObject> sender, CVS.CVSEvent eventArgs) {
							if(eventArgs.Modified is BlockTimeline) {
								BlockTimeline block = (BlockTimeline)eventArgs.Modified;
								switch(eventArgs.Type) {
								case CVS.CVSTypes.Delete:
									block.change-=changeBlock;
									Blocks.Remove(block);
									mBlocks[(int)block.Type-1].Remove(block);
									break;
								case CVS.CVSTypes.Info:
									mBlocks[(int)block.Type-1].Sort();
									break;
								}
								Frame(mParent.MiliSeconds,false);
							}
							OnChange(sender, eventArgs);
						}

						public UnityEngine.Texture getMiniScreenShot(string Scene) {
							if(mStrMini!=Scene) {
								mMini=resource().getMiniScreenShot(Scene);
								mStrMini=Scene;
							}
							return mMini;
						}
						
						public UnityEngine.Texture getMicroScreenShot(string Scene) {
							if(mStrMicro!=Scene) {
								mMicro=resource().getMicroScreenShot(Scene);
								mStrMicro=Scene;
							}
							return mMicro;
						}
						
						public UnityEngine.GameObject getInstance(string Scene) {
							if(mGameObject == null) {
								mGameObject = resource().getInstance(Scene);
								mGameObject.GetComponent<DataManager>().stageObject = this;
								mGameObject.GetComponent<DataManager>().isDone = true;
								
								if(KFPosition.Count > 0) {
									mGameObject.GetComponent<DataManager>().setPosition(KFPosition[0].Vector, KFPosition[0], null);
								} else if(initPos != null)
									mGameObject.GetComponent<DataManager>().setPosition(initPos.Vector, initPos, null);
								
								if(KFRotation.Count > 0) {
									mGameObject.GetComponent<DataManager>().Rotation = KFRotation[0].Vector;
								} else if(initRot != null)
									mGameObject.GetComponent<DataManager>().Rotation = initRot.Vector;
								
								if(KFScale.Count > 0) {
									mGameObject.GetComponent<DataManager>().Scale = KFScale[0].Vector;
								} else if(initSca != null)
									mGameObject.GetComponent<DataManager>().Scale = initSca.Vector;
							}
							return mGameObject;
						}

						public void setInstance(UnityEngine.GameObject camera) {
							if(mType != Types.Camera)
								throw new System.Exception("You only can set instance to a camera object.");
							if(mGameObject != null && mGameObject != camera)
								UnityEngine.MonoBehaviour.Destroy(mGameObject);

							mGameObject = camera;
							mGameObject.GetComponent<DataManager>().stageObject = this;
							mGameObject.GetComponent<DataManager>().isDone = true;

							if(KFPosition.Count > 0) {
								mGameObject.GetComponent<DataManager>().setPosition(KFPosition[0].Vector, KFPosition[0], null);
							} else if(initPos != null)
								mGameObject.GetComponent<DataManager>().setPosition(initPos.Vector, initPos, null);

							if(KFRotation.Count > 0) {
								mGameObject.GetComponent<DataManager>().Rotation = KFRotation[0].Vector;
							} else if(initRot != null)
								mGameObject.GetComponent<DataManager>().Rotation = initRot.Vector;

							if(KFScale.Count > 0) {
								mGameObject.GetComponent<DataManager>().Scale = KFScale[0].Vector;
							} else if(initSca != null)
								mGameObject.GetComponent<DataManager>().Scale = initSca.Vector;
						}

						private ResourcesLibrary.Resource3D resource() {
							switch(mType) {
								case Types.Background:
									return ResourcesLibrary.getBackground(mIdResource);
								case Types.Character:
									return ResourcesLibrary.getCharacter(mIdResource);
								case Types.Prop:
									return ResourcesLibrary.getProp(mIdResource);
								case Types.FX:
									return ResourcesLibrary.getFx(mIdResource);
								case Types.Camera:
									return ResourcesLibrary.getCamera(mIdResource);
							}
							return null;
						}
						
						public void Frame(float miliSeconds, bool play, bool omitCameraRot=false, bool player = false) {
							int frame = (int)(miliSeconds / Globals.MILISPERFRAME);

							if(mGameObject != null) {
								bool correct = false;
								Vector3 res;
								KeyFrame previousKF = null;
								KeyFrame nextKF = null;
								res = vectorFromList(KeyFrame.Types.Position, miliSeconds, out correct, out previousKF, out nextKF);
								if(correct)
									mGameObject.GetComponent<DataManager>().setPosition(res, previousKF, nextKF);
								else if(initPos != null)
									mGameObject.GetComponent<DataManager>().setPosition(initPos.Vector, initPos, null);
								
								if(!omitCameraRot || mType != Types.Camera) {
									res = vectorFromList(KeyFrame.Types.Rotation, miliSeconds, out correct, out previousKF, out nextKF);
									if(correct)
										mGameObject.GetComponent<DataManager>().Rotation = res;
									else if(initRot != null)
										mGameObject.GetComponent<DataManager>().Rotation = initRot.Vector;
								}
								
								res = vectorFromList(KeyFrame.Types.Scale, miliSeconds, out correct, out previousKF, out nextKF);
								if(correct)
									mGameObject.GetComponent<DataManager>().Scale = res;
								else if(initSca != null)
									mGameObject.GetComponent<DataManager>().Scale = initSca.Vector;

								bool activateIdle = true;
								List<BlockTimeline> blocksTemp = new List<BlockTimeline>(mBlocksPerfomed);
								foreach(BlockTimeline block in blocksTemp) {
									if(block.endAction(this, frame, play, player) || !Blocks.Contains(block)) {
										mBlocksPerfomed.Remove(block);
									}
									if(block.Type == BlockTimeline.Types.Animation) {
										activateIdle = false;
										mIdlePlaying = play;
									}
								}

								foreach(BlockTimeline block in Blocks) {
									if(block.performAction(this, frame, play, player)) {
										mBlocksPerfomed.Add(block);
										if(block.Type == BlockTimeline.Types.Animation) {
											activateIdle = false;
											mIdlePlaying = false;
										}
									}
								}
								if(mType == Types.Character && activateIdle && !mIdlePlaying) {
									int lFrame = lastFrame(BlockTimeline.Types.Animation, frame);
									mGameObject.GetComponent<DataManager>().IdleAnimation(lFrame, miliSeconds, play);
									mIdlePlaying = play;
								}
							}
						}
						
						public Vector3 vectorFromList(KeyFrame.Types type, float miliSeconds, out bool correct, out KeyFrame previousKF, out KeyFrame nextKF) {
							List<KeyFrame> lstKF = mKeyFrames[(int)type - 1];
							previousKF = null;
							nextKF = null;
							int count = -1;
							foreach(KeyFrame kf in lstKF) {
								if(kf.miliSecondsStart >= miliSeconds || Mathf.Approximately(kf.miliSecondsStart, miliSeconds)) {
									nextKF = kf;
									break;
								}
								previousKF = kf;
								count++;
								if(!kf.Control)
									count++;
							}

							if(previousKF != null && nextKF == null) {
								correct = true;
								return previousKF.Vector;
							} else if(previousKF == null && nextKF != null) {
								correct = true;
								return nextKF.Vector;
							} else if(previousKF != null && nextKF != null) {
								if(nextKF.Frame - previousKF.Frame == 1) {
									correct = true;
									if(Mathf.Approximately(miliSeconds, nextKF.miliSecondsStart)) {
										previousKF = null;
										return nextKF.Vector;
									} else {
										nextKF = null;
										return previousKF.Vector;
									}
								} else {
									correct = true;
									if(previousKF.Vector == nextKF.Vector) {
										nextKF = null;
										return previousKF.Vector;
									}
									float time = ((float)(miliSeconds - previousKF.miliSecondsStart) / (float)(nextKF.miliSecondsStart - previousKF.miliSecondsStart));
									if(time == 0) {
										nextKF = null;
										return previousKF.Vector;
									} else if(time == 1) {
										previousKF = null;
										return nextKF.Vector;
									} else {
										//time = Mathf.SmoothStep(0f,1f,time);
										return mBezierPath[(int)type - 1].CalculateBezierPoint(count, time);
									}
								}
							}
							correct = false;
							return new Vector3(0, 0, 0);
						}
						
						public void Reset(float miliSeconds) {
							if(mGameObject==null)
								return;
							int frame = (int)(miliSeconds/Globals.MILISPERFRAME);
							switch(mType) {
							case Types.Background:
								break;
							case Types.Camera:
								mGameObject.GetComponent<SceneCameraManager>().HideTexSprite2D(0);
								mGameObject.GetComponent<SceneCameraManager>().HideTexSprite2D(1);
								break;
							case Types.FX:
								mGameObject.GetComponent<FxControl>().dataPlay();
								GameObject obj2 = mGameObject.transform.Find("mesh").gameObject;
								foreach(Renderer ren in obj2.GetComponentsInChildren<Renderer>(true)) {
									ren.enabled=false;
								}
								//mGameObject.transform.Find("fx_plane").GetChild(0).gameObject.renderer.enabled=false;
								break;
							case Types.Character:
							case Types.Prop:
								GameObject obj = mGameObject.transform.Find("mesh").gameObject;
								mGameObject.GetComponent<DataManager>().isVisible=true;
								foreach(Renderer ren in obj.GetComponentsInChildren<Renderer>(true)) {
									ren.enabled=false;
									ren.material.color=Color.white;
								}
								if(mType==Types.Character){
									int lastFrame1 =  lastFrame(BlockTimeline.Types.Animation, frame);
									mGameObject.GetComponent<DataManager>().IdleAnimation(lastFrame1, frame * Globals.MILISPERFRAME, false);
									mGameObject.GetComponent<DataManager>().SetExpression("exp_base", false);
									mGameObject.GetComponent<DataManager>().ragDoll = false;
								}
								break;
							}
							
							mBlocksPerfomed.Clear();							
							foreach(BlockTimeline block in Blocks) {
								block.Reset(this, miliSeconds);
							}
						}
						
						public void Stop(float miliSeconds, bool Play) {
							bool invisible = !Play;
							List<BlockTimeline> blocksTemp = new List<BlockTimeline>(mBlocksPerfomed);
							foreach(BlockTimeline block in blocksTemp) {
								if(block.Type == BlockTimeline.Types.Visibility)
									invisible = false;
								if(block.Stop(this, Play))
									mBlocksPerfomed.Remove(block);
							}
							if(mIdlePlaying) {
								mIdlePlaying = false;
								mGameObject.GetComponent<DataManager>().StopAnimation();
							}
							//mBlocksPerfomed.Clear();
							
							if(invisible) {
								switch(mType) {
								case Types.FX:
									GameObject obj2 = mGameObject.transform.Find("mesh").gameObject;
									foreach(Renderer ren in obj2.GetComponentsInChildren<Renderer>(true)) {
										ren.enabled = true;
									}
									obj2 = mGameObject.transform.Find("fx_plane").GetChild(0).gameObject;
									obj2.renderer.material.color = Globals.colorInv;
									mGameObject.GetComponent<FxControl>().dataStop();
									break;
								case Types.Character:
								case Types.Prop:
									GameObject obj = mGameObject.transform.Find("mesh").gameObject;
									mGameObject.GetComponent<DataManager>().isVisible = false;
									foreach(Renderer ren in obj.GetComponentsInChildren<Renderer>(true)) {
										ren.enabled = true;
										ren.material.color = Globals.colorInv;
									}
									break;
								}
							}

							if(Play) {
								switch(mType) {
								case Types.FX:
									mGameObject.GetComponent<FxControl>().SetChildrenParticlesPause();
									break;
								case Types.Character:
								//case Types.Prop:
									mGameObject.GetComponent<DataManager>().StopAnimation();
									break;
								}
							}
						}
						
						public KeyFrame getKeyFrame(KeyFrame.Types type, int frame) {
							foreach(KeyFrame kf in mKeyFrames[(int)type - 1]) {
								if(kf.Frame==frame)
									return kf;
								else if(kf.Frame>frame)
									return null;
							}
							return null;
						}
						
						private BlockTimeline getBlock(BlockTimeline.Types type, int frame) {
							foreach(BlockTimeline block in mBlocks[(int)type - 1]) {
								if(block.StartFrame<=frame && block.EndFrame>frame)
									return block;
								else if(block.StartFrame>frame)
									return null;
							}
							return null;
						}
						
						private int lastFrame(BlockTimeline.Types type, int frame) {
							int ret = 0;
							foreach(BlockTimeline block in mBlocks[(int)type - 1]) {
								if(block.EndFrame <= frame)
									ret = block.EndFrame;
								else if(block.StartFrame < frame)
									break;
							}
							return ret;
						}

						internal void saveAudioClips(Dictionary<string, int> objects, int samples, byte[] emptySamplesBytes, int framesOffset, string path, System.IO.StreamWriter log, Export_Main export) {
							System.Threading.Thread t = new System.Threading.Thread(() => saveAudioClips2(objects, samples, emptySamplesBytes, framesOffset, path, log, export));
							QueueManager.add(new QueueManager.QueueManagerAction("Export", t.Start, "StageObject.saveAudioClips2"), QueueManager.Priorities.High);
							//QueueManager.add(new QueueManager.QueueManagerAction("Export", () => saveAudioClips2(objects, samples, emptySamplesBytes, framesOffset, path, log)));
						}
						//Se ejecuta en otro hilo.
						private void saveAudioClips2(Dictionary<string, int> objects, int samples, byte[] emptySamplesBytes, int framesOffset, string path, System.IO.StreamWriter log, Export_Main export) {
							export.Processing = true;
							string fileNameBase;
							if(mType == Types.Background)
								fileNameBase = "_Background";
							else if(mType == Types.Camera)
								fileNameBase = "_Camera";
							else
								fileNameBase = resource().Name;
							log.WriteLine("");
							log.WriteLine("Exportando audio de " + fileNameBase + ".");
							bool somethingExported = false;
							if(BSound.Count > 0 && !export.Abort) {
								if(!somethingExported) {
									fileNameBase = fileName(objects, fileNameBase, path);
									somethingExported = true;
								}
								saveAudioClips(samples, BlockTimeline.Types.Sound, fileNameBase + "_S.wav", emptySamplesBytes, framesOffset, log, export);
							}
							if(BMusic.Count > 0 && !export.Abort) {
								if(!somethingExported) {
									fileNameBase = fileName(objects, fileNameBase, path);
									somethingExported = true;
								}
								saveAudioClips(samples, BlockTimeline.Types.Music, fileNameBase + "_M.wav", emptySamplesBytes, framesOffset, log, export);
							}
							if(BVoice.Count > 0 && !export.Abort) {
								if(!somethingExported) {
									fileNameBase = fileName(objects, fileNameBase, path);
									somethingExported = true;
								}
								saveAudioClips(samples, BlockTimeline.Types.Voice, fileNameBase + "_V.wav", emptySamplesBytes, framesOffset, log, export);
							}
							if(!somethingExported && !export.Abort)
								log.WriteLine("El objeto " + fileNameBase + " no emite audio.");
							//log.WriteLine("");
							export.Processing = false;
						}

						private string fileName(Dictionary<string, int> objects, string fileNameBase, string path) {
							if(mType != Types.Background && mType != Types.Camera) {
								fileNameBase = resource().Name;
								if(objects.ContainsKey(fileNameBase))
									objects[fileNameBase] += 1;
								else
									objects.Add(fileNameBase, 1);
								fileNameBase =  fileNameBase + objects[fileNameBase].ToString("D3");
							}
							fileNameBase = System.IO.Path.Combine(path, fileNameBase);
							return fileNameBase;
						}

						//Se ejecuta en otro hilo, el hilo de saveAudioClips2.
						private void saveAudioClips(int samples, BlockTimeline.Types type, string fileName, byte[] emptySamplesBytes, int framesOffset, System.IO.StreamWriter log, Export_Main export) {
							const int maxSamples = 15 * Globals.OUTPUTRATEPERSECOND * Globals.NUMCHANNELS;
							if(!System.IO.File.Exists(fileName)) {
								log.WriteLine("Generado archivo " + fileName + ".");
								using(System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(fileName, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))) {
									writer.Write("RIFF".ToCharArray());
									writer.Write((samples * Globals.NUMCHANNELS * 2) + Globals.WAVHEADERSIZE - 8);
									writer.Write("WAVE".ToCharArray());
									writer.Write("fmt ".ToCharArray());
									writer.Write(16);
									writer.Write((short)1);
									writer.Write(Globals.NUMCHANNELS);
									writer.Write(Globals.OUTPUTRATEPERSECOND);
									writer.Write(Globals.OUTPUTRATEPERSECOND * 2 * Globals.NUMCHANNELS);
									writer.Write((short)(2 * Globals.NUMCHANNELS));
									writer.Write((short)16);
									writer.Write("data".ToCharArray());
									writer.Write(samples * Globals.NUMCHANNELS * 2);
									if(emptySamplesBytes != null)
										writer.Write(emptySamplesBytes);
									else {
										//byte[] empty = new byte[sizeof(short)];
										for(int i = 0; i < samples * Globals.NUMCHANNELS; ++i) {
											if(i % maxSamples == 0 && i > 0) {
												if(export.Abort)
													break;
												else
													writer.Flush();
											}
											writer.Write((short)0);
										}
									}
									writer.Close();
								}
							}
							if(!export.Abort) {
								List<BlockTimeline> blocks = null;
								switch(type) {
								case BlockTimeline.Types.Music:
									blocks = BMusic;
									break;
								case BlockTimeline.Types.Sound:
									blocks = BSound;
									break;
								case BlockTimeline.Types.Voice:
									blocks = BVoice;
									break;
								}
								foreach(BlockTimeline block in blocks) {
									if(export.Abort)
										break;
									BlockTimeline blk = block;
									QueueManager.add(new QueueManager.QueueManagerAction("Export", () => saveAudioClip(type, fileName, blk, framesOffset, log, export), "StageObject.saveAudioClip"), QueueManager.Priorities.Highest);
								}
							}
							//QueueManager.addGCCollectionMode(QueueManager.Priorities.Highest);
						}

						private void saveAudioClip(BlockTimeline.Types type, string fileName, BlockTimeline block, int framesOffset, System.IO.StreamWriter log, Export_Main export) {
							AudioClip clip = null;
							string soundName = "";
							switch(type) {
							case BlockTimeline.Types.Music:
								soundName = BRBRec.ResourcesLibrary.getMusic(block.IdResource).Name;
								clip = Resources.Load<AudioClip>(ResourcesLibrary.getMusic(block.IdResource).ResourceName);
								break;
							case BlockTimeline.Types.Sound:
								soundName = BRBRec.ResourcesLibrary.getSound(block.IdResource).Name;
								clip = Resources.Load<AudioClip>(ResourcesLibrary.getSound(block.IdResource).ResourceName);
								break;
							case BlockTimeline.Types.Voice:
								soundName = BRBRec.ResourcesLibrary.getVoice(block.IdResource).Name;
								if(block.IdResource > 0)
									clip = Resources.Load<AudioClip>(ResourcesLibrary.getVoice(block.IdResource).ResourceName);
								else
									clip = Data.dicRecordedSounds[block.IdResource].Sound;
								break;
							}
							float duration = Mathf.Min(clip.length, (block.EndFrame - block.StartFrame) * Globals.MILISPERFRAME);
							if(duration < 15) {
								float[] clipSamples = new float[Mathf.RoundToInt(duration * clip.channels * Globals.OUTPUTRATEPERSECOND)];
								clip.GetData(clipSamples, 0);
								int clipChannels = clip.channels;
								System.Threading.Thread t = new System.Threading.Thread(() => saveAudioClip2(fileName, soundName, block, framesOffset, clipSamples, clipChannels, log, export));
								QueueManager.add(new QueueManager.QueueManagerAction("Export", t.Start, "StageObject.saveAudioClip2"), QueueManager.Priorities.Highest);
							} else {
								saveAudioClip3(fileName, soundName, block, framesOffset, clip, log, export);
							}
							if(block.IdResource > 0)
								Resources.UnloadAsset(clip);
						}
						//Se ejecuta en otro hilo.
						private void saveAudioClip2(string fileName, string soundName, BlockTimeline block, int framesOffset, float[] clipSamples, int channels, System.IO.StreamWriter log, Export_Main export) {
							export.Processing = true;
							log.WriteLine("Insertando sonido " + soundName + "(" + block.StartFrame + " - " + block.EndFrame + ") en " + System.IO.Path.GetFileName(fileName) + ".");
							using(System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Write))) {
								writer.Seek(((block.StartFrame + framesOffset) * Globals.OUTPUTRATEPERFRAME * 2 * Globals.NUMCHANNELS) + Globals.WAVHEADERSIZE, System.IO.SeekOrigin.Begin);
								int sample = 0;
								if((block.EndFrame - block.StartFrame) * Globals.MILISPERFRAME < 15) {
									short[] shortSamples = new short[(block.EndFrame - block.StartFrame) * Globals.OUTPUTRATEPERFRAME * Globals.NUMCHANNELS];
									float fSample;
									for(int i = 0; i < (block.EndFrame - block.StartFrame) * Globals.OUTPUTRATEPERFRAME * channels; ++i, ++sample) {
										if(sample >= clipSamples.Length)
											sample = 0;
										if(block.controlValue != 10)
											fSample = Mathf.Clamp(clipSamples[sample], -1f, 1f) * block.controlValue / 10.0f;
										else
											fSample = Mathf.Clamp(clipSamples[sample], -1f, 1f);
										short sSample = (short)Math.Round((fSample * short.MaxValue));
										if(channels == 1) {
											shortSamples[i * 2] = sSample;
											shortSamples[(i * 2) + 1] = sSample;
										} else if(channels == 2)
											shortSamples[i] = sSample;
										sSample = (short)Mathf.Abs(sSample);
										if(export.HighestSample < sSample)
											export.HighestSample = sSample;
									}
									if(!export.Abort) {
										byte[] bytesSamples = new byte[shortSamples.Length * sizeof(short)];
										Buffer.BlockCopy(shortSamples, 0, bytesSamples, 0, bytesSamples.Length);
										writer.Write(bytesSamples);
									}
								} else {
									//Si dura más de 15 segundos escribir byte a byte.
									const int maxSamples = 15 * Globals.OUTPUTRATEPERSECOND * Globals.NUMCHANNELS;
									float fSample;
									for(int i = 0; i < (block.EndFrame - block.StartFrame) * Globals.OUTPUTRATEPERFRAME * channels; ++i, ++sample) {
										if(i % maxSamples == 0 && i > 0) {
											if(export.Abort)
												break;
											else
												writer.Flush();
										}
										if(sample >= clipSamples.Length)
											sample = 0;
										if(block.controlValue != 10)
											fSample = Mathf.Clamp(clipSamples[sample], -1f, 1f) * block.controlValue / 10.0f;
										else
											fSample = Mathf.Clamp(clipSamples[sample], -1f, 1f);
										short sSample = (short)Math.Round((fSample * short.MaxValue));
										if(channels == 1) {
											writer.Write(sSample);
											writer.Write(sSample);
										} else if(channels == 2)
											writer.Write(sSample);
										sSample = (short)Mathf.Abs(sSample);
										if(export.HighestSample < sSample)
											export.HighestSample = sSample;
									}
								}
								writer.Close();
							}
							export.Processing = false;
						}
						private void saveAudioClip3(string fileName, string soundName, BlockTimeline block, int framesOffset, AudioClip clip, System.IO.StreamWriter log, Export_Main export) {
							log.WriteLine("Insertando sonido " + soundName + "(" + block.StartFrame + " - " + block.EndFrame + ") en " + System.IO.Path.GetFileName(fileName) + ".");
							using(System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Write))) {
								writer.Seek(((block.StartFrame + framesOffset) * Globals.OUTPUTRATEPERFRAME * 2 * Globals.NUMCHANNELS) + Globals.WAVHEADERSIZE, System.IO.SeekOrigin.Begin);
								int sample = 0;
								float fSample;
								const int maxSamples = 15 * Globals.OUTPUTRATEPERSECOND * Globals.NUMCHANNELS;
								float[] clipSamples = new float[maxSamples];
								clip.GetData(clipSamples, 0);
								for(int i = 0; i < (block.EndFrame - block.StartFrame) * Globals.OUTPUTRATEPERFRAME * clip.channels; ++i, ++sample) {
									if(sample == maxSamples) {
										sample = 0;
										int offset = i / 2;//(i / maxSamples) * (maxSamples / 2);
										offset -= (offset / clip.samples) * clip.samples;
										clip.GetData(clipSamples, offset);
									}
									if(block.controlValue != 10)
										fSample = Mathf.Clamp(clipSamples[sample], -1f, 1f) * block.controlValue / 10.0f;
									else
										fSample = Mathf.Clamp(clipSamples[sample], -1f, 1f);
									short sSample = (short)Math.Round((fSample * short.MaxValue));
									if(clip.channels == 1) {
										writer.Write(sSample);
										writer.Write(sSample);
									} else if(clip.channels == 2)
										writer.Write(sSample);
									sSample = (short)Mathf.Abs(sSample);
									if(export.HighestSample < sSample)
										export.HighestSample = sSample;
									if(i % maxSamples == 0 && i > 0)
										writer.Flush();
								}
								writer.Close();
							}
						}

						public class KeyFrame : CVS.iCVSObject, System.IComparable<KeyFrame> {
							public enum Types {
								Position = 1,
								Rotation = 2,
								Scale = 3,
							}
							
							private int mIdKeyFrame;
							private StageObject mParent;
							private Types mType;
							public int Frame;
							public Vector3 Vector;
							public bool Control;

							private int mOldFrame;
							private Vector3 mOldVector;
							private bool mOldControl;
							
							public int IdKeyFrame {
								get { return mIdKeyFrame; }
							}
							public int IdStageObject {
								get { return mParent.IdObject; }
							}
							public Types Type {
								get { return mType; }
							}
							public float miliSecondsStart {
								get { return Frame * Globals.MILISPERFRAME; }
							}
							
							internal undoDelegate mUndoDelegate {
								get { return mParent.mUndoDelegate; }
							}
							
							internal KeyFrame(int IdKeyFrame, Types type, int frame, Vector3 vector, bool control, StageObject parent) {
								mIdKeyFrame=IdKeyFrame;
								mType=type;
								Frame=frame;
								Vector.x=vector.x;
								Vector.y=vector.y;
								Vector.z=vector.z;
								Control=control;
								mOldFrame=frame;
								mOldVector.x=vector.x;
								mOldVector.y=vector.y;
								mOldVector.z=vector.z;
								mOldControl=control;
								mParent=parent;
							}
							
							public void save() {
								if(mIdKeyFrame==-1) {
									if(Frame==-1) 
										db.ExecuteNonQuery("INSERT INTO KeyFrames (IdObject, IdKeyFrameType, CVSNew) VALUES (" + mParent.IdObject + ", " + (int)mType + ", -1)");
									else
										db.ExecuteNonQuery("INSERT INTO KeyFrames (IdObject, IdKeyFrameType, CVSNew) VALUES (" + mParent.IdObject + ", " + (int)mType + ", " + mParent.Version + ")");
									mIdKeyFrame=(int)db.ExecuteQuery("SELECT MAX(IdKeyFrame) as ID FROM KeyFrames")[0]["ID"];
									db.ExecuteNonQuery("UPDATE KeyFramesInfo SET Frame = " + Frame + ", X = " + Vector.x + ", Y = " + Vector.y + ", Z = " + Vector.z + ", Control = " + System.Convert.ToInt32(Control) + " WHERE IdKeyFrame = " + mIdKeyFrame + " AND CVSNew = -1");
									CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Create, this);
									if(Frame!=-1) 
										OnChange (evt);
								} else {
									if(Frame==-1)
										throw new System.Exception("You can't modify an init frame.");
									
									if(mOldFrame==Frame && mOldVector.x==Vector.x && mOldVector.y==Vector.y && mOldVector.z==Vector.z && mOldControl==Control)
										return;
									db.ExecuteNonQuery("UPDATE KeyFramesInfo SET CVSDel = " + mParent.Version + " WHERE IdKeyFrame = " + mIdKeyFrame + " AND CVSDel IS NULL");
									db.ExecuteNonQuery("INSERT INTO KeyFramesInfo (IdKeyFrame, CVSNew, Frame, X, Y, Z, Control) VALUES (" + mIdKeyFrame + ", " + mParent.Version + ", " + Frame + ", " + Vector.x + ", " + Vector.y + ", " + Vector.z + ", " + System.Convert.ToInt32(Control) + ")");
									CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Info, this);
									OnChange(evt);
									mOldFrame=Frame;
									mOldVector.x=Vector.x;
									mOldVector.y=Vector.y;
									mOldVector.z=Vector.z;
									mOldControl=Control;
								}
							}

							public void delete() {
								if(Frame==-1)
									throw new System.Exception("You can't delete an init frame.");

								db.ExecuteNonQuery("UPDATE KeyFrames SET CVSDel = " + mParent.Version + " WHERE IdKeyFrame = " + mIdKeyFrame + " AND CVSDel IS NULL");
								CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Delete, this);
								OnChange (evt);
							}
							
							internal override bool undoLocal(CVS cvs) { 
								switch(cvs.Type) {
								case CVS.CVSTypes.Create:
									db.ExecuteNonQuery("DELETE FROM KeyFrames WHERE IdKeyFrame = " + mIdKeyFrame + " AND CVSNew = " + cvs.Version);
									return true;
								case CVS.CVSTypes.Delete:
									db.ExecuteNonQuery("UPDATE KeyFrames SET CVSDel = NULL WHERE IdKeyFrame = " + mIdKeyFrame + " AND CVSDel = " + cvs.Version);
									return true;
								case CVS.CVSTypes.Info:
									DataRow row = db.ExecuteQuery("SELECT Frame, X, Y, Z, Control FROM KeyFramesInfo WHERE IdKeyFrame = " + mIdKeyFrame + " AND CVSDel = " + cvs.Version)[0];
									Frame=(int)row["Frame"];
									mOldFrame=(int)row["Frame"];
									Vector.x=System.Convert.ToSingle(row["X"]);
									Vector.y=System.Convert.ToSingle(row["Y"]);
									Vector.z=System.Convert.ToSingle(row["Z"]);
									Control=System.Convert.ToBoolean(row["Control"]);
									mOldVector.x=System.Convert.ToSingle(row["X"]);
									mOldVector.y=System.Convert.ToSingle(row["Y"]);
									mOldVector.z=System.Convert.ToSingle(row["Z"]);
									mOldControl=System.Convert.ToBoolean(row["Control"]);
									db.ExecuteNonQuery("DELETE FROM KeyFramesInfo WHERE IdKeyFrame = " + mIdKeyFrame + " AND CVSDel IS NULL");
									db.ExecuteNonQuery("UPDATE KeyFramesInfo SET CVSDel = NULL WHERE IdKeyFrame = " + mIdKeyFrame + " AND CVSDel = " + cvs.Version);
									return true;
								}
								return false;
							}
						
							public int CompareTo(KeyFrame other) {
								if ( this.Frame < other.Frame ) return -1;
								else if ( this.Frame > other.Frame ) return 1;
								else return 0;
							}
						}
						
						public class BlockTimeline : System.IComparable<BlockTimeline> {
							public enum Types {
								Visibility = 1,
								Animation = 2,
								Expression = 3, //Si es estatico, comportamiento como el de visibilidad.
								Sound = 4,
								Voice = 5,
								Music = 6,
								Prop1 = 7, //Si es estatico, comportamiento como el de visibilidad.
								Prop2 = 8, //Si es estatico, comportamiento como el de visibilidad.
								Prop3 = 9, //Si es estatico, comportamiento como el de visibilidad.
								Sprite2D1 = 10,
								Sprite2D2 = 11,
							}
							
							protected int mIdBlockTimeline;
							protected StageObject mParent;
							protected Types mType;
							//internal virtual event changeEvent change;
							protected int mIdResource;
							public int StartFrame;
							public int Frames;
							protected int mControl; //Positive true - Negative false

							protected int mOldStartFrame;
							protected int mOldFrames;
							protected int mPerformed;
							protected int mOldControl;
							
							public int IdBlockTimeline {
								get { return mIdBlockTimeline; }
							}
							public int IdStageObject {
								get { return mParent.IdObject; }
							}
							public int IdResource {
								get { return mIdResource; }
							}
							public Types Type {
								get { return mType; }
							}
							public float miliSecondsStart {
								get { return StartFrame * Globals.MILISPERFRAME; }
							}
							public float miliSecondsDuration {
								get { return Frames * Globals.MILISPERFRAME; }
							}
							public float miliSecondsEnds {
								get { return EndFrame * Globals.MILISPERFRAME; }
							}
							public int EndFrame {
								get { return StartFrame + Frames; }
							}
							public bool Performed {
								get { return mPerformed!=-1; }
							}

							public bool Control {
								get {
									if(mControl > 0)
										return true;
									else
										return false;
								}
								set {
									if(value)
										mControl = Mathf.Abs(mControl);
									else
										mControl = Mathf.Abs(mControl) * -1;
								}
							}
							public int controlValue {
								get { return Mathf.Abs(mControl); }
								set {
									if(value <= 0)
										throw new System.Exception("Control value must be a value greater than 0.");
									if(Control)
										mControl = value;
									else
										mControl = value * -1;
								}
							}

							internal BlockTimeline(int IdBlockTimeline, Types type, int idResource, int startFrame, int frames, int control, StageObject parent) {
								mIdBlockTimeline=IdBlockTimeline;
								mType=type;
								mIdResource=idResource;
								StartFrame=startFrame;
								Frames=frames;
								mControl=control;
								mOldStartFrame=startFrame;
								mOldFrames=frames;
								mOldControl=control;
								mParent=parent;
								mPerformed=-1;
							}
							
							public virtual void save() {
								if(mIdBlockTimeline == -1) {
									if(mType == Types.Visibility && mParent.BVisibility.Count == 0)
										db.ExecuteNonQuery("INSERT INTO BlocksTimeline (IdObject, IdBlockType, CVSNew, IdResource) VALUES (" + mParent.IdObject + ", " + (int)mType + ", -1, " + mIdResource + ")");
									else
										db.ExecuteNonQuery("INSERT INTO BlocksTimeline (IdObject, IdBlockType, CVSNew, IdResource) VALUES (" + mParent.IdObject + ", " + (int)mType + ", " + mParent.Version + ", " + mIdResource + ")");
									mIdBlockTimeline = (int)db.ExecuteQuery("SELECT MAX(IdBlockTimeline) as ID FROM BlocksTimeline")[0]["ID"];
									db.ExecuteNonQuery("UPDATE BlocksTimelineInfo SET StartFrame = " + StartFrame + ", Frames = " + Frames + ", Control = " + mControl + " WHERE IdBlockTimeline = " + mIdBlockTimeline + " AND CVSNew = -1");
									CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Create, this);
									OnChange(evt);
								} else {
									if(mOldStartFrame == StartFrame && mOldFrames == Frames && mOldControl == mControl)
										return;
									db.ExecuteNonQuery("UPDATE BlocksTimelineInfo SET CVSDel = " + mParent.Version + " WHERE IdBlockTimeline = " + mIdBlockTimeline + " AND CVSDel IS NULL");
									db.ExecuteNonQuery("INSERT INTO BlocksTimelineInfo (IdBlockTimeline, CVSNew, StartFrame, Frames, Control) VALUES (" + mIdBlockTimeline + ", " + mParent.Version + ", " + StartFrame + ", " + Frames + ", " + mControl + ")");
									CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Info, this);
									if(mPerformed >= 0) {
										mPerformed++;
									}
									OnChange(evt);
									if(Performed) {
										if(mType == Types.Voice && mOldControl > 0 && mControl != mOldControl) {
											GameObject gameObj = mParent.getInstance("Scene");
											gameObj.GetComponent<DataManager>().stopLipSync();
										} else if((mType == Types.Sprite2D1 || mType == Types.Sprite2D2) && mControl != mOldControl) {
											mPerformed = -3;
											performAction(mParent, (int)(mParent.mParent.MiliSeconds / Globals.MILISPERFRAME), false, false);
										}
									}
									mOldStartFrame = StartFrame;
									mOldFrames = Frames;
									mOldControl = mControl;
								}
							}
							
							public void delete() {
								//Si es el último de visibilidad eliminamos el objeto.
								if(mType == Types.Visibility && mParent.BVisibility.Count == 1) {
									mParent.delete();
									return;
								}
								
								if(Performed) {
									switch(mType) {
									case Types.Visibility:
										GameObject gameObj = mParent.getInstance("Scene");
										if(mParent.Type == Data.Episode.Scene.Stage.StageObject.Types.FX) {
											GameObject obj = gameObj.transform.Find("fx_plane").GetChild(0).gameObject;
											obj.renderer.material.color = Globals.colorInv;
										} else {
											GameObject obj = gameObj.transform.Find("mesh").gameObject;
											gameObj.GetComponent<DataManager>().isVisible = false;
											foreach(Renderer ren in obj.GetComponentsInChildren<Renderer>(true)) {
												ren.material.color = Globals.colorInv;
											}
										}
										break;
									case Types.Voice:
										if(Control) {
											GameObject gameObj3 = mParent.getInstance("Scene");
											gameObj3.GetComponent<DataManager>().stopLipSync();
										}
										break;
									case Types.Animation:
										int frame = (int)(mParent.mParent.MiliSeconds / Globals.MILISPERFRAME);
										GameObject gameObj1 = mParent.getInstance("Scene");
										int lastFrame = mParent.lastFrame(mType, frame);
										gameObj1.GetComponent<DataManager>().IdleAnimation(lastFrame, mParent.mParent.MiliSeconds, false);
										break;
									case Types.Expression:
										GameObject gameObj2 = mParent.getInstance("Scene");
										gameObj2.GetComponent<DataManager>().SetExpression("exp_base", false);
										break;
									case Types.Sprite2D1:
										Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().HideTexSprite2D(0);
										break;
									case Types.Sprite2D2:
										Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().HideTexSprite2D(1);
										break;
									case Types.Prop1:
									case Types.Prop2:
									case Types.Prop3:
										((BlockTimelineProp)this).getInstance().SetActive(false);
										break;
									}
								}
								
								db.ExecuteNonQuery("UPDATE BlocksTimeline SET CVSDel = " + mParent.Version + " WHERE IdBlockTimeline = " + mIdBlockTimeline + " AND CVSDel IS NULL");
								CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Delete, this);
								OnChange(evt);
								mPerformed = -1;
							}
							
							public int CompareTo(BlockTimeline other) {
								if(this.StartFrame < other.StartFrame)
									return -1;
								else if(this.StartFrame > other.StartFrame)
									return 1;
								else {
									if(mType == Types.Voice)
										return 1;
									else if(other.Type == Types.Voice)
										return -1;
								}
								return 0;
							}
							
							public virtual bool performAction(StageObject staObject, int frame, bool play, bool player) {
								if(StartFrame <= frame && EndFrame > frame && mPerformed != frame && mPerformed != -2) {
									GameObject gameObj = staObject.getInstance("Scene");
									switch(mType) {
									case Types.Animation:
										//float sec = (frame - StartFrame) * Globals.MILISPERFRAME;
										bool ret = mPerformed == -1;
										if(mPerformed != -2) {
											gameObj.GetComponent<DataManager>().PlayAnimation(BRBRec.ResourcesLibrary.getAnimation(mParent.IdResource, IdResource).ResourceName, StartFrame, frame * Globals.MILISPERFRAME, play, Control);
											if(play)
												mPerformed = -2;
											else
												mPerformed = frame;
										}
										//mPerformed=frame;
										return ret;
									case Types.Expression:
										gameObj.GetComponent<DataManager>().SetExpression(BRBRec.ResourcesLibrary.getExpression(mParent.IdResource, IdResource).ResourceName, false);
										mPerformed = -2;
										return true;
									case Types.Music:
									case Types.Sound:
										if(play) {
											float secAD = (frame - StartFrame) * Globals.MILISPERFRAME;
											gameObj.GetComponent<DataManager>().PlayAudio(IdResource, mType, secAD, controlValue);
											mPerformed = -2;
											return true;
										}
										break;
									case Types.Voice:
										bool ret1 = mPerformed == -1;
										if(play) {
											float secAD1 = (frame - StartFrame) * Globals.MILISPERFRAME;
											gameObj.GetComponent<DataManager>().PlayAudio(IdResource, mType, secAD1, controlValue);
											if(Control)
												gameObj.GetComponent<DataManager>().startLipSync();
											mPerformed = -2;
											return ret1;
										} else if(Control) {
											AudioClip clip;
											if(IdResource > 0)
												clip = ResourcesManager.LoadResource(ResourcesLibrary.getVoice(IdResource).ResourceName, "Scene") as AudioClip;
											else
												clip = Data.dicRecordedSounds[IdResource].Sound;
											gameObj.GetComponent<DataManager>().lipSync(clip, frame, StartFrame, controlValue);
											mPerformed = frame;
											return ret1;
										}
										break;
									case Types.Sprite2D1:
									case Types.Sprite2D2:
										int tex = (int)mType - (int)Types.Sprite2D1;
										bool ret2 = mPerformed == -1;
										if(mPerformed == -1) {
											Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().ShowTexSprite2D(IdResource, tex);
											Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().alphaSprite2D((play || player) ? 1.0f : 0.5f, tex);
											Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().resetSprite2D(tex);
											mPerformed = -2;
										}
										if(Control) {
											float percentage = ((float)(frame - StartFrame) / (EndFrame - StartFrame));
											switch(controlValue) {
											case 1:
												Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().fadeSpriteIn(miliSecondsDuration, percentage, play, tex, player);
												break;
											case 2:
												Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().fadeSpriteOut(miliSecondsDuration, percentage, play, tex, player);
												break;
											case 3:
												Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().scrollSpriteLeft(miliSecondsDuration, percentage, play, tex, player);
												break;
											case 4:
												Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().scrollSpriteUp(miliSecondsDuration, percentage, play, tex, player);
												break;
											}
											if(!play)
												mPerformed = frame;
										} else if(mPerformed == -3) {
											Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().resetSprite2D(tex);
											Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().alphaSprite2D((play || player) ? 1.0f : 0.5f, tex);
											mPerformed = -2;
										}
										return ret2;
									case Types.Visibility:
										if(staObject.Type == Data.Episode.Scene.Stage.StageObject.Types.FX) {
											GameObject obj;
											if(play) {
												obj = gameObj.transform.Find("mesh").gameObject;
												foreach(Renderer ren in obj.GetComponentsInChildren<Renderer>(true)) {
													ren.enabled = true;
												}
											}
											obj = gameObj.transform.Find("fx_plane").GetChild(0).gameObject;
											obj.renderer.material.color = Color.white;
										} else {
											if(staObject.Type==Data.Episode.Scene.Stage.StageObject.Types.Character){
												if(play){
													gameObj.GetComponent<DataManager>().ragDoll= Control;
												}
											}
											GameObject obj = gameObj.transform.Find("mesh").gameObject;
											gameObj.GetComponent<DataManager>().isVisible = true;
											foreach(Renderer ren in obj.GetComponentsInChildren<Renderer>(true)) {
												ren.enabled = true;
												ren.material.color = Color.white;
											}
										}
										mPerformed = -2;
										return true;
									}
								}
								return false;
							}
							public virtual bool endAction(StageObject staObject, int frame, bool play, bool player) {
								if((StartFrame > frame || EndFrame <= frame) && mPerformed != -1) {
									GameObject gameObj = staObject.getInstance("Scene");
									switch(mType) {
									case Types.Animation:
										int lastFrame;
										if(EndFrame == frame)
											lastFrame = EndFrame;
										else
											lastFrame = mParent.lastFrame(mType, frame);
										gameObj.GetComponent<DataManager>().IdleAnimation(lastFrame, frame * Globals.MILISPERFRAME, play);
										break;
									case Types.Expression:
										if(frame != Data.selStage.mFrames || (frame == Data.selStage.mFrames && EndFrame != Data.selStage.mFrames))
											gameObj.GetComponent<DataManager>().SetExpression("exp_base", false);
										else
											return false;
										break;
									case Types.Music:
									case Types.Sound:
										gameObj.GetComponent<DataManager>().StopAudio(mType);
										break;
									case Types.Voice:
										if(mPerformed == -2)
											gameObj.GetComponent<DataManager>().StopAudio(mType);
										if(Control)
											gameObj.GetComponent<DataManager>().stopLipSync();
										break;
									case Types.Sprite2D1:
										if(frame != Data.selStage.mFrames || (frame == Data.selStage.mFrames && EndFrame != Data.selStage.mFrames))
											Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().HideTexSprite2D(0);
										else
											return false;
										break;
									case Types.Sprite2D2:
										if(frame != Data.selStage.mFrames || (frame == Data.selStage.mFrames && EndFrame != Data.selStage.mFrames))
											Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().HideTexSprite2D(1);
										else
											return false;
										break;
									case Types.Visibility:
										if(frame != Data.selStage.mFrames || (frame == Data.selStage.mFrames && EndFrame != Data.selStage.mFrames)) {
											if(staObject.Type == Data.Episode.Scene.Stage.StageObject.Types.FX) {
												if(play || player) {
													GameObject obj = gameObj.transform.Find("mesh").gameObject;
													foreach(Renderer ren in obj.GetComponentsInChildren<Renderer>(true)) {
														ren.enabled = false;
													}
												} else {
													GameObject obj = gameObj.transform.Find("fx_plane").GetChild(0).gameObject;
													obj.renderer.material.color = Globals.colorInv;
												}
											} else {
												if(staObject.Type == Data.Episode.Scene.Stage.StageObject.Types.Character) {
													if(play) {
														gameObj.GetComponent<DataManager>().ragDoll=false;
													}
												}
												
												GameObject obj = gameObj.transform.Find("mesh").gameObject;
												if(!(play || player))
													gameObj.GetComponent<DataManager>().isVisible = false;
												foreach(Renderer ren in obj.GetComponentsInChildren<Renderer>(true)) {
													if(play || player) {
														ren.enabled = false;
													} else {
														ren.material.color = Globals.colorInv;
													}
												}
											}
										} else
											return false;
										break;
									}
									mPerformed = -1;
									return true;
								}
								return false;
							}
							public virtual bool Stop(StageObject staObject, bool Play) {
								if(mPerformed != -1) {
									GameObject gameObj = staObject.getInstance("Scene");
									switch(mType) {
									case Types.Animation:
										gameObj.GetComponent<DataManager>().StopAnimation();
										mPerformed = 1;
										return false;
									case Types.Music:
									case Types.Sound:
										gameObj.GetComponent<DataManager>().StopAudio(mType);
										break;
									case Types.Voice:
										gameObj.GetComponent<DataManager>().StopAudio(mType);
										mPerformed = -3;
										return false;
									case Types.Sprite2D1:
										Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().stopSprite(Play, 0);
										mPerformed = -3;
										return false;
									case Types.Sprite2D2:
										Data.selStage.SceneCamera.mGameObject.GetComponent<SceneCameraManager>().stopSprite(Play, 1);
										mPerformed = -3;
										return false;
									case Types.Visibility:
										if(staObject.Type==Data.Episode.Scene.Stage.StageObject.Types.Character){
											if(gameObj.GetComponent<DataManager>().ragDoll) {
												gameObj.GetComponent<DataManager>().ragDoll = false;
												gameObj.GetComponent<DataManager>().StopAnimation();
											}
										}
										return false;
									case Types.Expression:
									case Types.Prop1:
									case Types.Prop2:
									case Types.Prop3:
										return false;
									}
									mPerformed = -1;
								}
								return true;
							}
							public virtual void Reset(StageObject staObject, float time) {
								GameObject gameObj = staObject.getInstance("Scene");
								if(Performed) {
									switch(mType) {
									case Types.Animation:
										if(miliSecondsStart <= time && miliSecondsEnds > time) {
											//gameObj.GetComponent<DataManager>().ForceAnimation(BRBRec.ResourcesLibrary.getAnimation(mParent.IdResource, IdResource).ResourceName, time - miliSecondsStart, Control);
											gameObj.GetComponent<DataManager>().PlayAnimation(BRBRec.ResourcesLibrary.getAnimation(mParent.IdResource, IdResource).ResourceName, StartFrame, time, false, Control);
										} else {
											//gameObj.GetComponent<DataManager>().ForceAnimation("idle", time, false);
											int frame = (int)(mParent.mParent.MiliSeconds/Globals.MILISPERFRAME);
											int lastFrame =  mParent.lastFrame(mType, frame);
											gameObj.GetComponent<DataManager>().IdleAnimation(lastFrame, time, false);
										}
										break;
									case Types.Music:
									case Types.Sound:
										gameObj.GetComponent<DataManager>().StopAudio(mType);
										break;
									case Types.Voice:
										gameObj.GetComponent<DataManager>().StopAudio(mType);
										if(Control)
											gameObj.GetComponent<DataManager>().stopLipSync();
										break;
									}
								}
								mPerformed = -1;
							}
						}
						
						public class BlockTimelineProp : BlockTimeline {
							public string Dummy;
							public Vector3 Rotation;
							public Vector3 Position;
							public Vector3 Scale;
							//internal override event changeEvent change;
							
							private string mOldDummy;
							public Vector3 mOldRotation;
							public Vector3 mOldPosition;
							public Vector3 mOldScale;
							
							private GameObject mGoProp;

							internal BlockTimelineProp(int IdBlockTimeline, Types type, int idResource, int startFrame, int frames, int control, StageObject parent, string dummy, Vector3 position, Vector3 rotation, Vector3 scale)
								: base(IdBlockTimeline, type, idResource, startFrame, frames, control, parent) {
								Dummy=dummy;
								Position = position;
								Rotation = rotation;
								Scale = scale;

								mOldDummy=dummy;
								mOldPosition = position;
								mOldRotation=rotation;
								mOldScale = scale;
								mOldControl=control;
							}
							
							public UnityEngine.GameObject getInstance() {
								if(mGoProp == null) {
									mGoProp = ResourcesLibrary.getCharProp(mIdResource).getInstance("Scene");
									Renderer ren = mParent.mGameObject.transform.Find("mesh").gameObject.GetComponentsInChildren<Renderer>(true)[0];
									foreach(Renderer ren1 in mGoProp.GetComponentsInChildren<Renderer>(true)) {
										ren1.enabled = ren.enabled;
										ren1.material.color = ren.material.color;
									}									

									Transform tBodyPart = Utils.FindInHierarchy(mParent.getInstance("Scene"), Dummy);
									mGoProp.transform.parent = tBodyPart;
									//Vector3 trans;
									//trans = Position;// new Vector3(tBodyPart.position.x + Position.x, tBodyPart.position.y + Position.y, tBodyPart.position.z + Position.z);
									mGoProp.transform.localPosition = Position;

									//trans = Rotation;// new Vector3(tBodyPart.eulerAngles.x + Rotation.x, tBodyPart.eulerAngles.y + Rotation.y, tBodyPart.eulerAngles.z + Rotation.z);
									mGoProp.transform.localEulerAngles = Rotation;

									//trans =  new Vector3(Scale.x, Scale.y, Scale.z);
									mGoProp.transform.localScale = Scale;
								}
								return mGoProp;
							}
							
							public override bool performAction(StageObject staObject, int frame, bool play, bool player) {
								if(StartFrame <= frame && EndFrame > frame && mPerformed != -2) {
									mGoProp = getInstance();
									mGoProp.SetActive(true);
									mPerformed = -2;
									return true;
								}
								return false;
							}
							
							public override bool endAction(StageObject staObject, int frame, bool play, bool player) {
								if(((StartFrame > frame || EndFrame <= frame) && mPerformed != -1)
								   && (frame != Data.selStage.mFrames || (frame == Data.selStage.mFrames && EndFrame != Data.selStage.mFrames))) {
									mGoProp = getInstance();
									mGoProp.SetActive(false);
									mPerformed = -1;
									return true;
								}
								return false;
							}

							public void destroyInstancePlayer() {
								if(mGoProp != null) {
									MonoBehaviour.Destroy(mGoProp);
									mGoProp = null;
								}
							}
							
							public override void save() {
								if(mIdBlockTimeline == -1) {
									db.ExecuteNonQuery("INSERT INTO BlocksTimeline (IdObject, IdBlockType, CVSNew, IdResource) VALUES (" + mParent.IdObject + ", " + (int)mType + ", " + mParent.Version + ", " + mIdResource + ")");
									mIdBlockTimeline = (int)db.ExecuteQuery("SELECT MAX(IdBlockTimeline) as ID FROM BlocksTimeline")[0]["ID"];
									db.ExecuteNonQuery("UPDATE BlocksTimelineInfo SET StartFrame = " + StartFrame + ", Frames = " + Frames + ", Control = " + mControl + " WHERE IdBlockTimeline = " + mIdBlockTimeline + " AND CVSNew = -1");
									db.ExecuteNonQuery("INSERT INTO BlocksTimelineProp (IdBlockTimeline, CVSNew, Dummy, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, ScaleX, ScaleY, ScaleZ) VALUES (" + mIdBlockTimeline + ", -1, '" + Dummy + "', " + Position.x + ", " + Position.y + ", " + Position.z + ", " + Rotation.x + ", " + Rotation.y + ", " + Rotation.z + ", " + Scale.x + ", " + Scale.y + ", " + Scale.z + ")");
									CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Create, this);
									OnChange(evt);
								} else {
									bool saved = false;
									if(mOldStartFrame != StartFrame || mOldFrames != Frames && mOldControl == mControl) {
										db.ExecuteNonQuery("UPDATE BlocksTimelineInfo SET CVSDel = " + mParent.Version + " WHERE IdBlockTimeline = " + mIdBlockTimeline + " AND CVSDel IS NULL");
										db.ExecuteNonQuery("INSERT INTO BlocksTimelineInfo (IdBlockTimeline, CVSNew, StartFrame, Frames, Control) VALUES (" + mIdBlockTimeline + ", " + mParent.Version + ", " + StartFrame + ", " + Frames + ", " + mControl + ")");
										saved = true;
										mOldStartFrame = StartFrame;
										mOldFrames = Frames;
									}
									if(mOldDummy != Dummy || mOldPosition != Position || mOldRotation != Rotation || mOldScale != Scale) {
										db.ExecuteNonQuery("UPDATE BlocksTimelineProp SET CVSDel = " + mParent.Version + " WHERE IdBlockTimeline = " + mIdBlockTimeline + " AND CVSDel IS NULL");
										db.ExecuteNonQuery("INSERT INTO BlocksTimelineProp (IdBlockTimeline, CVSNew, Dummy, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, ScaleX, ScaleY, ScaleZ) VALUES (" + mIdBlockTimeline + ", " + mParent.Version + ", '" + Dummy + "', " + Position.x + ", " + Position.y + ", " + Position.z + ", " + Rotation.x + ", " + Rotation.y + ", " + Rotation.z + ", " + Scale.x + ", " + Scale.y + ", " + Scale.z + ")");
										saved = true;
										mOldDummy = Dummy;
										mOldPosition = Position;
										mOldRotation = Rotation;
										mOldScale = Scale;

										if(mGoProp != null) {
											Transform tBodyPart = Utils.FindInHierarchy(mParent.getInstance("Scene"), Dummy);
											mGoProp.transform.parent = tBodyPart;
											Vector3 trans;
											trans = new Vector3(tBodyPart.position.x + Position.x, tBodyPart.position.y + Position.y, tBodyPart.position.z + Position.z);
											mGoProp.transform.position = trans;

											trans = new Vector3(tBodyPart.eulerAngles.x + Rotation.x, tBodyPart.eulerAngles.y + Rotation.y, tBodyPart.eulerAngles.z + Rotation.z);
											mGoProp.transform.eulerAngles = trans;

											trans = new Vector3(tBodyPart.localScale.x * Scale.x, tBodyPart.localScale.y * Scale.y, tBodyPart.localScale.z * Scale.z);
											mGoProp.transform.localScale = trans;
										}
									}
									if(saved) {
										CVS.CVSEvent evt = new CVS.CVSEvent(CVS.CVSTypes.Info, this);
										OnChange(evt);
									}
								}
							}
						}
					}
				}
			}
		}*/
	}

	public interface iObject {
		int Id {
			get;
		}
		int Number {
			get;
		}

		void Save();
		void Delete();
		bool Change();
		void Revert();
	}
}