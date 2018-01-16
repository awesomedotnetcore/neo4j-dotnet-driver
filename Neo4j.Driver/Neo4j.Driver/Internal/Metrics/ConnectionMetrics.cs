﻿// Copyright (c) 2002-2018 "Neo Technology,"
// Network Engine for Objects in Lund AB [http://neotechnology.com]
//
// This file is part of Neo4j.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using HdrHistogram;
using static Neo4j.Driver.Internal.Metrics.Histogram;

namespace Neo4j.Driver.Internal.Metrics
{
    internal class ConnectionMetrics : IConnectionMetrics, IConnectionListener
    {
        public string UniqueName { get; }
        public IHistogram ConnectionTimeHistogram { get; }
        public IHistogram InUseTimeHistogram { get; }

        private readonly LongConcurrentHistogram _connectionTimeHistogram;
        private readonly LongConcurrentHistogram _inUseTimeHistogram;

        public ConnectionMetrics(Uri uri, TimeSpan connectionTimeout)
        {
            UniqueName = uri.ToString();

            if (connectionTimeout.IsTimeoutDetectionDisabled())
            {
                connectionTimeout = DefaulHighestTrackable;
            }
            _connectionTimeHistogram = new LongConcurrentHistogram(1, connectionTimeout.Ticks, 0);
            _inUseTimeHistogram = new LongConcurrentHistogram(1, DefaulHighestTrackable.Ticks, 0);

            ConnectionTimeHistogram = new Histogram(_connectionTimeHistogram);
            InUseTimeHistogram = new Histogram(_inUseTimeHistogram);
        }

        public void BeforeConnect(IListenerEvent connEvent)
        {
            connEvent.Start();
        }

        public void AfterConnect(IListenerEvent connEvent)
        {
            var newValue = TruncateValue(connEvent.GetElapsed(), _connectionTimeHistogram);
            _connectionTimeHistogram.RecordValue(newValue);
        }

        public void OnAcquire(IListenerEvent connEvent)
        {
            connEvent.Start();
        }

        public void OnRelease(IListenerEvent connEvent)
        {
            var newValue = TruncateValue(connEvent.GetElapsed(), _inUseTimeHistogram);
            _inUseTimeHistogram.RecordValue(newValue);
        }


    }

}
