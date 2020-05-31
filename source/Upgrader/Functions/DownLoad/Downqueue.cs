using System;
using System.Collections.Generic;

namespace Zoranner.Upgrader
{
    public class Downqueue
    {
        private readonly List<Downloader> _DoneList;
        private readonly List<Downloader> _LoadList;

        //public bool InLoading { get; private set; }
        //public bool Completed { get; } = false;

        public int Count => _DoneList.Count + _LoadList.Count;

        public TaskState State { get; private set; }

        public float Progress
        {
            get
            {
                if (Current != null)
                {
                    return (_DoneList.Count + Current.Progress) / (_DoneList.Count + _LoadList.Count);
                }

                return 0;
            }
        }

        public Downloader Current { get; private set; }

        public string Caption => Current == null ? null : $"{Current.Name} ({(int) (Current.Progress * 100)}%)";

        public event DownloadStateChangedHandler StateChangedEvent;

        public Downqueue()
        {
            _LoadList = new List<Downloader>();
            _DoneList = new List<Downloader>();
        }

        public void Add(Downloader downloader)
        {
            _LoadList.Add(downloader);
        }

        public void Add(string httpUrl, string httpHash, string localPath)
        {
            _LoadList.Add(new Downloader(httpUrl, httpHash, localPath));
        }

        public void Start()
        {
            if (State == TaskState.Loading)
            {
                return;
            }

            //StartedEvent?.Invoke(this);
            SetState(TaskState.Loading, 0f);
            NextStart();
        }

        public void Pause()
        {
            if (State != TaskState.Loading)
            {
                return;
            }

            Current.Pause();
        }

        public void Delete(Downloader downloader)
        {
            if (!_LoadList.Contains(downloader))
            {
                return;
            }

            downloader.Delete();
            _LoadList.Remove(downloader);
        }

        private void NextStart()
        {
            if (Current != null)
            {
                _DoneList.Add(Current);
                _LoadList.Remove(Current);
                Current.StateChangedEvent -= Current_StateChangedEvent;
            }

            if (_LoadList.Count > 0)
            {
                Current = _LoadList[0];
                Current.StateChangedEvent += Current_StateChangedEvent;
                Current.Start();
            }
            else
            {
                Current = null;
                Finish();
            }
        }

        private void SetState(TaskState state, object message = null)
        {
            State = state;
            StateChangedEvent?.Invoke(State, message);
        }

        public void Clear()
        {
            while (_LoadList.Count > 0)
            {
                _LoadList[0].Delete();
                _LoadList[0] = null;
                _LoadList.Remove(_LoadList[0]);
            }

            while (_DoneList.Count > 0)
            {
                _DoneList[0].Delete();
                _DoneList[0] = null;
                _DoneList.Remove(_DoneList[0]);
            }
        }

        private void Finish()
        {
            SetState(TaskState.Finished);
        }

        private void Current_StateChangedEvent(TaskState state, object message = null)
        {
            switch (state)
            {
                case TaskState.Prepared:
                    //SetState(TaskState.Loading, Progress);
                    break;
                case TaskState.Loading:
                    SetState(TaskState.Loading, Progress);
                    break;
                case TaskState.Paused:
                    SetState(TaskState.Paused);
                    break;
                case TaskState.Broken:
                    SetState(TaskState.Broken, message);
                    break;
                case TaskState.Finished:
                    NextStart();
                    SetState(TaskState.Loading, Progress);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}