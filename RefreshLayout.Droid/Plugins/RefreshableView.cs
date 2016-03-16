using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace RefreshLayout.Droid
{
    public class RefreshableView : LinearLayout
    {
        private Context mContext;


        #region 构造方法


        public RefreshableView(Context context) : base(context)
        {
            mContext = context;
            Init();
        }

        public RefreshableView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            mContext = context;
            Init();
        }


        private View FooterView
        {
            get
            {
                if (_footerView == null)
                {

                    LayoutParams lp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, 60)
                    {
                        Gravity = GravityFlags.Center
                    };
                    _footerView = LayoutInflater.From(mContext).Inflate(Resource.Layout.XListViewFooter, null);
                    AddView(_footerView, lp);
                }
                return _footerView;
            }
        }
        #endregion 构造方法

        #region 私有变量

        private const string TAG = "LILITH";
        private Scroller scroller;
        private View refreshView;
        private ImageView refreshIndicatorView;
        private int refreshTargetTop = -60;
        private ProgressBar bar;
        private TextView downTextView;
        private TextView timeTextView;
        private LinearLayout reFreshTimeLayout; //显示上次刷新时间的layout


        private View _footerView;

        public delegate void RefreshableEventHandler(RefreshableView view);
        public event RefreshableEventHandler OnRefresh;
        public event RefreshableEventHandler OnLoadMore;

        private string downTextString;
        private string releaseTextString;

        private long? RefreshTime { get; set; } = null;
        private int lastX;
        private int lastY;
        // 拉动标记
        private bool isDragging = false;
        // 是否可刷新标记
        public bool IsRefreshEnabled { get; set; } = true;
        // 在刷新中标记
        private bool isRefreshing = false;
        DateTime LastRefreshTime;


        #endregion 私有变量


        #region 私有方法

        private void Init()
        {
            // TODO Auto-generated method stub
            //滑动对象，
            LastRefreshTime = DateTime.Now;
            scroller = new Scroller(mContext);

            //刷新视图顶端的的view
            refreshView = LayoutInflater.From(mContext).Inflate(Resource.Layout.refresh_top_item, null);
            //指示器view
            refreshIndicatorView = (ImageView)refreshView.FindViewById(Resource.Id.indicator);
            //刷新bar
            bar = (ProgressBar)refreshView.FindViewById(Resource.Id.progress);
            //下拉显示text
            downTextView = (TextView)refreshView.FindViewById(Resource.Id.refresh_hint);
            //下来显示时间
            timeTextView = (TextView)refreshView.FindViewById(Resource.Id.refresh_time);



            LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, -refreshTargetTop)
            {
                TopMargin = refreshTargetTop,
                Gravity = GravityFlags.Center
            };
            AddView(refreshView, lp);
            downTextString = mContext.Resources.GetString(Resource.String.refresh_down_text);
            releaseTextString = mContext.Resources.GetString(Resource.String.refresh_release_text);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="time"></param>
        private void SetRefreshText()
        {
            // TODO Auto-generated method stub
            timeTextView.Text = DateTime.Now.ToString("g");
        }


        /// <summary>
        /// up事件处理
        /// </summary>
        private void Fling()
        {
            isDragging = true;
            // TODO Auto-generated method stub
            LinearLayout.LayoutParams lp = (LayoutParams)refreshView.LayoutParameters;

            if (lp.TopMargin > 0)
            {
                //拉到了触发可刷新事件
                Refresh();
                Log.Info(TAG, "Fling()" + lp.TopMargin);
            }
            else if (lp.BottomMargin > 0)
            {
                LoadMoreData();
                Log.Info(TAG, "Fling()" + lp.BottomMargin);
            }
            else
            {
                ReturnInitState();
            }
        }



        private void ReturnInitState()
        {
            // TODO Auto-generated method stub
            LayoutParams lp = (LinearLayout.LayoutParams)this.refreshView.LayoutParameters;
            int i = lp.TopMargin;
            scroller.StartScroll(0, i, 0, refreshTargetTop);
            Invalidate();
        }

        private void Refresh()
        {
            // TODO Auto-generated method stub
            LayoutParams lp = (LayoutParams)this.refreshView.LayoutParameters;
            int i = lp.TopMargin;
            refreshIndicatorView.Visibility = ViewStates.Gone;
            bar.Visibility = ViewStates.Visible;
            timeTextView.Visibility = ViewStates.Gone;
            downTextView.Visibility = ViewStates.Gone;
            scroller.StartScroll(0, i, 0, 0 - i);
            Invalidate();
            if (OnRefresh != null)
            {
                OnRefresh.Invoke(this);
                isRefreshing = true;
            }
        }


        private void LoadMoreData()
        {
            // TODO Auto-generated method stub
            LayoutParams lp = (LayoutParams)this.refreshView.LayoutParameters;
            int i = lp.BottomMargin;
            scroller.StartScroll(0, i, 0, 0 - i);
            Invalidate();
            if (OnRefresh != null)
            {
                OnLoadMore?.Invoke(this);
                isRefreshing = true;
            }
        }
        /// <summary>
        /// 下拉move事件处理
        /// </summary>
        /// <param name="moveY"></param>
        private void DoMovement(int moveY)
        {
            // TODO Auto-generated method stub
            LayoutParams lp = (LayoutParams)refreshView.LayoutParameters;
            if (moveY > 0)
            {
                //获取view的上边距
                float f1 = lp.TopMargin;
                float f2 = moveY * 0.3F;
                int i = (int)(f1 + f2);
                //修改上边距
                lp.TopMargin = i;
                //修改后刷新
                refreshView.LayoutParameters = lp;
                refreshView.Invalidate();
                Invalidate();
            }
            timeTextView.Visibility = ViewStates.Visible;
            if (RefreshTime != null)
            {
                SetRefreshText();
            }
            downTextView.Visibility = ViewStates.Visible;

            refreshIndicatorView.Visibility = ViewStates.Visible;
            bar.Visibility = ViewStates.Gone;
            if (lp.TopMargin > 0)
            {
                downTextView.SetText(Resource.String.refresh_release_text);
                refreshIndicatorView.SetImageResource(Resource.Drawable.refresh_arrow_up);
            }
            else
            {
                downTextView.SetText(Resource.String.refresh_down_text);
                refreshIndicatorView.SetImageResource(Resource.Drawable.refresh_arrow_down);
            }

        }


        private void DoPull(int moveY)
        {
            LayoutParams lp = (LayoutParams)refreshView.LayoutParameters;
            if (moveY < 0)
            {
                //获取view的上边距
                float f1 = lp.BottomMargin;
                float f2 = moveY * 0.3F;
                int i = (int)(f1 - f2);
                //修改上边距
                lp.BottomMargin = i;
                //修改后刷新
                FooterView.LayoutParameters = lp;
                FooterView.Invalidate();
                Invalidate();
            }
        }


        private bool CanScroll()
        {
            // TODO Auto-generated method stub
            if (ChildCount > 1)
            {
                var childView = this.GetChildAt(1);
                if (childView is ListView)
                {
                    int top = ((ListView)childView).GetChildAt(0).Top;
                    int pad = ((ListView)childView).ListPaddingTop;
                    if ((Math.Abs(top - pad)) < 3 &&
                        ((ListView)childView).FirstVisiblePosition == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (childView is ScrollView)
                {
                    if (((ScrollView)childView).ScrollY == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        #endregion 构造方法

        #region Override

        public override bool OnTouchEvent(MotionEvent e)
        {
            int y = (int)e.RawY;


            switch (e.Action)
            {
                case MotionEventActions.Down:
                    //记录下y坐标
                    lastY = y;
                    break;

                case MotionEventActions.Move:
                    Log.Info(TAG, "ACTION_MOVE");
                    //y移动坐标
                    int m = y - lastY;

                    if (((m < 6) && (m > -1)))
                    {
                        isPull = false;
                        DoMovement(m);
                    }
                    else if (m < -1)
                    {
                        isPull = true;
                        DoPull(m);
                    }

                    //记录下此刻y坐标
                    this.lastY = y;
                    break;

                case MotionEventActions.Up:
                    Log.Info(TAG, "ACTION_UP");

                    Fling();

                    break;
            }
            return true;
        }

        /// <summary>
        /// 该方法一般和ontouchEvent 一起用
        /// (non-Javadoc)
        /// @see android.view.ViewGroup#onInterceptTouchEvent(android.view.MotionEvent)
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            var action = e.Action;
            var y = (int)e.RawY;
            switch (action)
            {
                case MotionEventActions.Down:
                    lastY = y;
                    break;

                case MotionEventActions.Cancel:
                    break;
                case MotionEventActions.Move:
                    //y移动坐标
                    int m = y - lastY;

                    //记录下此刻y坐标
                    this.lastY = y;
                    if (m > 6 && CanScroll())
                    {
                        return true;
                    }
                    else if (m < -6 && !CanScroll())
                    {
                        Console.WriteLine("Pull ------------- Padding={0},CanScroll={1}", m, CanScroll());
                        return true;
                    }
                    Log.Info("Test", m.ToString());
                    Console.WriteLine("Padding={0},CanScroll={1}", m, CanScroll());

                    break;
                case MotionEventActions.Up:
                    break;

            }
            return false;
        }

        private bool isPull = false;

        public override void ComputeScroll()
        {
            if (scroller.ComputeScrollOffset())
            {

                if (isPull)
                {

                    PullComputeScroll();
                }
                else
                    DownComputeScroll();
                Invalidate();
            }
        }

        #endregion Override


        public void PullComputeScroll()
        {
            int i = this.scroller.CurrY;
            FooterView.Visibility = ViewStates.Gone;
            this.FooterView.Invalidate();
        }

        public void DownComputeScroll()
        {
            int i = this.scroller.CurrY;
            LayoutParams lp = (LayoutParams)this.refreshView.LayoutParameters;
            int k = Math.Max(i, refreshTargetTop);
            lp.TopMargin = k;
            this.refreshView.LayoutParameters = lp;
            this.refreshView.Invalidate();
            Invalidate();
        }

        /// <summary>
        ///  结束刷新事件
        /// </summary>
        public void FinishRefresh()
        {
            Log.Info(TAG, "执行了=====FinishRefresh");
            LinearLayout.LayoutParams lp = (LinearLayout.LayoutParams)this.refreshView.LayoutParameters;
            int i = lp.TopMargin;
            i = 0;
            refreshIndicatorView.Visibility = ViewStates.Visible;
            timeTextView.Visibility = ViewStates.Visible;
            scroller.StartScroll(0, i, 0, refreshTargetTop);
            Invalidate();
            isRefreshing = false;
        }

    }
}