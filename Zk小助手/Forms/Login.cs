﻿using log4net;
using System;
using System.Windows.Forms;
using ZkManager.Forms;
using ZooKeeperNet;

namespace ZkManager
{
    public partial class Login : Form
    {
        /// <summary>
        /// 日志类
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(Login));

        /// <summary>
        /// 本地配置项类
        /// </summary>
        private Config config;
        public Login(Config config)
        {
            InitializeComponent();
            this.config = config;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            foreach (string key in config.GetKets())
            {
                ListBox_IpList.Items.Add(key);
            }
            
        }

        private void Button_Add_Click(object sender, EventArgs e)
        {
            Edit editForm = new Edit(config);
            DialogResult result = editForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                ListBox_IpList.Items.Add(editForm.name);
            }
        }

        private void Button_Edit_Click(object sender, EventArgs e)
        {
            // 获取当前选中项
            int index = ListBox_IpList.SelectedIndex;

            if (-1 == index)
                return;

            string name = ListBox_IpList.Items[index].ToString();
            string value = config[name];
            Edit editForm = new Edit(name, value, config);

            DialogResult result = editForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                ListBox_IpList.Items.Remove(name);
                ListBox_IpList.Items.Add(editForm.name);
            }
        }

        private void Button_Delete_Click(object sender, EventArgs e)
        {
            int index = ListBox_IpList.SelectedIndex;
            // 没有选择东西则返回
            if (-1 == index)
            {
                return;
            }

            string value = (string)ListBox_IpList.Items[index];

            // 删除配置项
            config.Remove(value);
            ListBox_IpList.Items.RemoveAt(index);

            index--;

            // 范围判断
            if (index <= -1 || index >= ListBox_IpList.Items.Count)
            {
                return;
            }

            ListBox_IpList.SelectedIndex = index;
        }

        private void Button_Login_Click(object sender, EventArgs e)
        {
            int index = ListBox_IpList.SelectedIndex;
            // 没有选择东西则返回
            if (-1 == index)
            {
                return;
            }

            string name = (string)ListBox_IpList.Items[index];
            string value = config[name];
            ZkClient client = new ZkClient(value, 3000);
            if (!client.IsConected)
            {
                MessageBox.Show("无法连接到指定IP。");
                client.close();
                return;
            }

            Main frm_Main = new Main(client, name);
            // 隐藏登陆窗体
            this.Hide();

            // Yes 是重新连接，No 是直接退出
            DialogResult result = DialogResult.No ;
            try
            {
                result = frm_Main.ShowDialog();
            }
            catch (KeeperException ex)
            {
                log.Error("KeeperException", ex);
            }

            // 如果用户选择不是重新连接
            if (DialogResult.Yes != result)
            {
                Close();
                return;
            }

            // 用户选择了重新连接，重新展示该窗口
            this.Show();
        }
        
    }
}
