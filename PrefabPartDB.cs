﻿// ToadicusTools
//
// PrefabPartDB.cs
//
// Copyright © 2014, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using KSP;
using System;
using System.Collections.Generic;

namespace ToadicusTools
{
	public class PrefabPartDB
	{
		private static PrefabPartDB _instance;

		/// <summary>
		/// Gets the ModuleDB instance for the specified type
		/// </summary>
		/// <value>The ModuleDB instance for the specified type</value>
		public static PrefabPartDB Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new PrefabPartDB();
				}

				return _instance;
			}
		}

		protected Dictionary<string, Dictionary<string, AvailablePart.ModuleInfo>> partModuleNameDB;

		#if BENCH
		public int cacheHits { get; protected set; }
		public int cacheMisses { get; protected set; }
		#endif

		private PrefabPartDB()
		{
			this.partModuleNameDB = new Dictionary<string, Dictionary<string, AvailablePart.ModuleInfo>>();
			#if BENCH
			this.cacheHits = 0;
			this.cacheMisses = 0;
			GameEvents.onGameSceneLoadRequested.Add(this.onSceneChange);
			#endif
		}

		public Dictionary<string, AvailablePart.ModuleInfo> getPrefabModuleDB(string partName)
		{
			AvailablePart partPrefab = PartLoader.getPartInfoByName(partName);

			if (partPrefab != null)
			{
				if (!this.partModuleNameDB.ContainsKey(partName))
				{
					#if BENCH
					this.cacheMisses++;
					#endif
					Dictionary<string, AvailablePart.ModuleInfo> prefabModuleDB =
						new Dictionary<string, AvailablePart.ModuleInfo>();

					foreach (AvailablePart.ModuleInfo moduleInfo in partPrefab.moduleInfos)
					{
						prefabModuleDB[moduleInfo.moduleName] = moduleInfo;
					}

					this.partModuleNameDB[partName] = prefabModuleDB;
				}
				#if BENCH
				else
				{
					this.cacheHits++;
				}
				#endif

				return this.partModuleNameDB[partName];
			}

			return new Dictionary<string, AvailablePart.ModuleInfo>();
		}

		#if BENCH
		private void onSceneChange(GameScenes scene)
		{
			int cacheSwings = this.cacheHits + this.cacheMisses;
			KSPLog.print(string.Format("PrefabPartDB Destructing.  Cache hits: {0} ({1}%), cache misses: {2} ({3}%).",
				this.cacheHits, (float)this.cacheHits / (float)cacheSwings * 100f,
				this.cacheMisses, (float)this.cacheMisses / (float)cacheSwings * 100f
			));
		}
		#endif
	}
}