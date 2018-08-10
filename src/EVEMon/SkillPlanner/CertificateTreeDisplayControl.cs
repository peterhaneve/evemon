using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Helpers;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;

namespace EVEMon.SkillPlanner
{
    /// <summary>
    /// UserControl to display a tree of certificates.
    /// </summary>
    public sealed partial class CertificateTreeDisplayControl : UserControl
    {
        private const string TrainedIcon = "Trained";
        private const string TrainableIcon = "Trainable";
        private const string UntrainableIcon = "Untrainable";
        private const string SkillBookIcon = "Skillbook";
        private const string PlannedIcon = "Planned";

        // Blank image list for 'Safe for work' setting
        private readonly ImageList m_emptyImageList = new ImageList();

        private Plan m_plan;
        private Character m_character;
        private CertificateClass m_class;
        private Font m_boldFont;

        private bool m_allExpanded;


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateTreeDisplayControl"/> class.
        /// </summary>
        public CertificateTreeDisplayControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.ContainerControl |
                     ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            InitializeComponent();
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets or sets the current plan.
        /// </summary>
        internal Plan Plan
        {
            get { return m_plan; }
            set
            {
                m_plan = value;
                if (m_plan == null)
                    return;

                m_character = (Character)m_plan.Character;
                UpdateTree();
            }
        }

        /// <summary>
        /// Gets or sets the certificate class (i.e. "Core competency").
        /// </summary>
        internal CertificateClass CertificateClass
        {
            get { return m_class; }
            set
            {
                if (m_class == value)
                    return;

                m_class = value;
                m_character = m_class?.Character;
                UpdateTree();
            }
        }

        /// <summary>
        /// Gets cert of the displayed class which contains the current selection.
        /// </summary>
        private CertificateLevel SelectedCertificateLevel
        {
            get
            {
                TreeNode node = treeView.SelectedNode;
                while (node != null)
                {
                    CertificateLevel certLevel = node.Tag as CertificateLevel;
                    if (certLevel != null && certLevel.Certificate.Class == m_class)
                        return certLevel;

                    node = node.Parent;
                }
                return null;
            }
        }

        #endregion

        
        #region Event Handlers

        /// <summary>
        /// On load, complete the initialization.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Return on design mode
            if (DesignMode || this.IsDesignModeHosted())
                return;

            m_boldFont = FontFactory.GetFont(Font, FontStyle.Bold);
            treeView.Font = FontFactory.GetFont("Microsoft Sans Serif", 8.25F);
            treeView.ItemHeight = treeView.Font.Height * 2 + 6;

            m_emptyImageList.ImageSize = new Size(30, 24);
            m_emptyImageList.Images.Add(new Bitmap(30, 24));

            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;
            EveMonClient.CharacterUpdated += EveMonClient_CharacterUpdated;
            EveMonClient.PlanChanged += EveMonClient_PlanChanged;
            Disposed += OnDisposed;
        }

        /// <summary>
        /// Unsubscribe events on disposing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            EveMonClient.CharacterUpdated -= EveMonClient_CharacterUpdated;
            EveMonClient.PlanChanged -= EveMonClient_PlanChanged;
            Disposed -= OnDisposed;
        }

        /// <summary>
        /// On settings change, we update the tree.
        /// </summary>
        /// <remarks>Relates to safe for work setting</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            UpdateTree();
        }

        /// <summary>
        /// Fired when one of the character changed (skill completion, update from CCP, etc).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (m_plan == null)
                return;

            if (e.Character != m_plan.Character)
                return;

            UpdateTree();
        }

        /// <summary>
        /// Occurs when the plan changes, we update the tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_PlanChanged(object sender, PlanChangedEventArgs e)
        {
            if ((e.Plan != m_plan) || (e.Plan.Character != m_plan.Character))
                return;

            UpdateTree();
        }

        /// <summary>
        /// When the treeview is clicked, we manually select nodes since the bounding boxes are incorrect due to custom draw.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                treeView.Cursor = Cursors.Default;

            // Perform the selection manually since the bound's width and x are incorrect in owndraw
            TreeNode selection = null;
            for (TreeNode node = treeView.TopNode; node != null; node = node.NextVisibleNode)
            {
                if (node.Bounds.Top > e.Y || node.Bounds.Bottom < e.Y)
                    continue;

                // If the user clicked the "arrow zone", we do not change the selection and just return
                if (e.X < node.Bounds.Left - 32)
                    return;

                selection = node;
                break;
            }

            treeView.SelectedNode = selection;
        }

        /// <summary>
        /// When the mouse moves over the list, we change the cursor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;

            treeView.Cursor = CustomCursors.ContextMenu;
        }
        
        /// <summary>
        /// Event handler for treenode double click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is CertificateLevel)
            {
                if (e.Node.IsExpanded)
                {
                    e.Node.Collapse();
                    return;
                }

                e.Node.Expand();
            }

            showInBrowserMenu_Click(sender, e);
        }

        #endregion


        #region Tree building

        /// <summary>
        /// Update the whole tree.
        /// </summary>
        private void UpdateTree()
        {
            CertificateLevel oldSelection = SelectedCertificateLevel;
            TreeNode newSelection = null;

            treeView.ImageList = Settings.UI.SafeForWork ? m_emptyImageList : imageList;

            treeView.BeginUpdate();
            try
            {
                // Clear the old items
                treeView.Nodes.Clear();

                // No update when not fully initialized
                if (m_class == null)
                    return;

                // Create the nodes when not done, yet
                if (treeView.Nodes.Count == 0)
                {
                    foreach (CertificateLevel certLevel in m_class.Certificate.AllLevel)
                    {
                        TreeNode node = CreateNode(certLevel);
                        treeView.Nodes.Add(node);

                        // Does the old selection still exists ?
                        if (certLevel == oldSelection)
                            newSelection = node;
                    }
                }

                // Update the nodes
                foreach (TreeNode node in treeView.Nodes)
                {
                    UpdateNode(node);
                }

                // Is the old selection kept ? Then we select the matching node
                if (newSelection != null)
                    treeView.SelectedNode = newSelection;
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        /// <summary>
        /// Create a node from a prerequisite certificate.
        /// </summary>
        /// <param name="certLevel">The cert level.</param>
        /// <returns></returns>
        private static TreeNode CreateNode(CertificateLevel certLevel)
        {
            TreeNode node = new TreeNode
            {
                Text = certLevel.ToString(),
                Tag = certLevel
            };

            foreach (SkillLevel prereqSkill in certLevel.PrerequisiteSkills)
            {
                node.Nodes.Add(CreateNode(prereqSkill));
            }

            return node;
        }

        /// <summary>
        /// Create a node from a prerequisite skill.
        /// </summary>
        /// <param name="skillPrereq"></param>
        /// <returns></returns>
        private static TreeNode CreateNode(SkillLevel skillPrereq)
        {
            TreeNode node = new TreeNode
            {
                Text = skillPrereq.ToString(),
                Tag = skillPrereq
            };

            // Add this skill's prerequisites
            foreach (SkillLevel prereqSkill in skillPrereq.Skill.Prerequisites
                .Where(prereqSkill => prereqSkill.Skill != skillPrereq.Skill))
            {
                node.Nodes.Add(CreateNode(prereqSkill));
            }

            return node;
        }

        /// <summary>
        /// Updates the specified node and its children.
        /// </summary>
        /// <param name="node"></param>
        private void UpdateNode(TreeNode node)
        {
            CertificateLevel certLevel = node.Tag as CertificateLevel;

            // The node represents a certificate
            if (certLevel != null)
            {
                if (m_character != null)
                {
                    if (certLevel.IsTrained)
                        node.ImageIndex = imageList.Images.IndexOfKey(TrainedIcon);
                    else if (certLevel.IsPartiallyTrained)
                        node.ImageIndex = imageList.Images.IndexOfKey(TrainableIcon);
                    else
                        node.ImageIndex = imageList.Images.IndexOfKey(UntrainableIcon);
                }
            }
            // The node represents a skill prerequisite
            else
            {
                SkillLevel skillPrereq = (SkillLevel)node.Tag;
                Skill skill = m_character?.Skills[skillPrereq.Skill.ID] ?? skillPrereq.Skill;

                if (m_character != null)
                {
                    if (skillPrereq.IsTrained)
                        node.ImageIndex = imageList.Images.IndexOfKey(TrainedIcon);
                    else if (m_plan != null && m_plan.IsPlanned(skill, skillPrereq.Level))
                        node.ImageIndex = imageList.Images.IndexOfKey(PlannedIcon);
                    else if (skill.IsKnown)
                        node.ImageIndex = imageList.Images.IndexOfKey(TrainableIcon);
                    else
                        node.ImageIndex = imageList.Images.IndexOfKey(UntrainableIcon);
                }
            }

            // When selected, the image remains the same
            node.SelectedImageIndex = node.ImageIndex;

            // Update the children
            foreach (TreeNode child in node.Nodes)
            {
                UpdateNode(child);
            }
        }

        /// <summary>
        /// Custom draw for the label.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // Prevents a bug that causes every item to be redrawn at the top left corner
            if (e.Bounds.Left <= 10)
            {
                e.DrawDefault = true;
                return;
            }

            string line2 = "-";
            int supIcon = -1;
            ImageList il;

            // Is it a certificate level ?
            CertificateLevel certLevel = e.Node.Tag as CertificateLevel;
            if (certLevel != null)
            {
                if (!Settings.UI.SafeForWork)
                    supIcon = (int)certLevel.Level;

                il = imageListCertLevels;

                // When not trained, let's display the training time
                if (!certLevel.IsTrained)
                {
                    TimeSpan time = certLevel.GetTrainingTime;
                    if (time != TimeSpan.Zero)
                        line2 = time.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas);
                }
            }
            // Or a skill prerequisite ?
            else
            {
                if (!Settings.UI.SafeForWork)
                    supIcon = imageList.Images.IndexOfKey(SkillBookIcon);

                il = imageList;
                SkillLevel skillPrereq = (SkillLevel)e.Node.Tag;

                // When not known to the require level, let's display the training time
                if (!skillPrereq.IsTrained)
                {
                    TimeSpan time = skillPrereq.Skill.GetLeftTrainingTimeToLevel(skillPrereq.Level);
                    if (time != TimeSpan.Zero)
                        line2 = time.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas);
                }
            }

            // Choose colors according to selection
            bool isSelected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            Color backColor = isSelected ? SystemColors.Highlight : treeView.BackColor;
            Color foreColor = isSelected ? SystemColors.HighlightText : treeView.ForeColor;
            Color lightForeColor = isSelected ? SystemColors.HighlightText : SystemColors.GrayText;

            // Draws the background
            using (SolidBrush background = new SolidBrush(backColor))
            {
                int width = treeView.ClientSize.Width - e.Bounds.Left;
                e.Graphics.FillRectangle(background, new Rectangle(e.Bounds.Left, e.Bounds.Top, width, e.Bounds.Height));
            }

            // Performs the drawing
            using (SolidBrush foreground = new SolidBrush(foreColor))
            {
                float offset;
                int left = e.Bounds.Left + il.ImageSize.Width + 2;
                Size line1Size = TextRenderer.MeasureText(e.Node.Text, m_boldFont);

                if (!String.IsNullOrEmpty(line2))
                {
                    Size line2Size = TextRenderer.MeasureText(line2, Font);

                    offset = (float)(e.Bounds.Height - line1Size.Height - line2Size.Height) / 2;
                    e.Graphics.DrawString(e.Node.Text, m_boldFont, foreground, new PointF(left, e.Bounds.Top + offset));

                    using (SolidBrush lightForeground = new SolidBrush(lightForeColor))
                    {
                        e.Graphics.DrawString(line2, Font, lightForeground, new PointF(left, e.Bounds.Top + offset + line1Size.Height));
                    }
                }
                else
                {
                    offset = (float)(e.Bounds.Height - line1Size.Height) / 2;
                    e.Graphics.DrawString(e.Node.Text, m_boldFont, foreground, new PointF(left, e.Bounds.Top + offset));
                }
            }

            // Draws the icon for skill/cert on the far right
            if (supIcon == -1)
                return;

            int imgOfssetX = e.Bounds.Left;
            float imgOffsetY = Math.Max(0.0f, (e.Bounds.Height - il.ImageSize.Height) * 0.5f);
            e.Graphics.DrawImageUnscaled(il.Images[supIcon], imgOfssetX, (int)(e.Bounds.Top + imgOffsetY));
        }

        #endregion


        #region Context menus

        /// <summary>
        /// Context menu opening, we update the menus' statuses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmListSkills_Opening(object sender, CancelEventArgs e)
        {
            TreeNode node = treeView.SelectedNode;
            CertificateLevel certLevel = node?.Tag as CertificateLevel;

            planToLevel.Visible = planToLevelSeparator.Visible = m_plan != null && node != null;

            if (m_plan != null)
            {
                // When a certificate is selected
                if (certLevel != null)
                {
                    // Update "add to" menu
                    planToLevel.Enabled = !m_plan.WillGrantEligibilityFor(certLevel);
                    planToLevel.Text = $"Plan \"{certLevel}\"";
                }
                // When a skill is selected
                else if (node != null)
                {
                    // Update "add to" menu
                    SkillLevel prereq = (SkillLevel)node.Tag;
                    Skill skill = prereq.Skill;
                    planToLevel.Enabled = skill.Level < prereq.Level && !m_plan.IsPlanned(skill, prereq.Level);
                    planToLevel.Text = $"Plan \"{skill} {Skill.GetRomanFromInt(prereq.Level)}\"";
                }
            }

            // Update "show in" menu
            showInBrowserMenu.Visible =
                showInExplorerMenu.Visible =
                    showInMenuSeparator.Visible = node != null && certLevel == null;

            // "Collapse" and "Expand" menus
            tsmCollapseSelected.Visible = node != null && node.GetNodeCount(true) > 0 && node.IsExpanded;
            tsmExpandSelected.Visible = node != null && node.GetNodeCount(true) > 0 && !node.IsExpanded;

            tsmExpandSelected.Text = node != null && node.GetNodeCount(true) > 0 && !node.IsExpanded
                ? $"Expand \"{node.Text}\""
                : String.Empty;
            tsmCollapseSelected.Text = node != null && node.GetNodeCount(true) > 0 && node.IsExpanded
                ? $"Collapse \"{node.Text}\""
                : String.Empty;

            toggleSeparator.Visible = node != null && node.GetNodeCount(true) > 0;

            // "Expand All" and "Collapse All" menus
            tsmCollapseAll.Enabled = tsmCollapseAll.Visible = m_allExpanded;
            tsmExpandAll.Enabled = tsmExpandAll.Visible = !tsmCollapseAll.Enabled;
        }

        /// <summary>
        /// Treeview's context menu > Plan "(selection)".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmAddToPlan_Click(object sender, EventArgs e)
        {
            CertificateLevel cert = treeView.SelectedNode.Tag as CertificateLevel;
            IPlanOperation operation;

            if (cert != null)
                operation = m_plan.TryPlanTo(cert);
            else
            {
                SkillLevel prereq = (SkillLevel)treeView.SelectedNode.Tag;
                operation = m_plan.TryPlanTo(prereq.Skill, prereq.Level);
            }

            if (operation == null)
                return;

            PlanWindow planWindow = ParentForm as PlanWindow;
            if (planWindow == null)
                return;

            PlanHelper.SelectPerform(new PlanToOperationWindow(operation), planWindow, operation);
        }

        /// <summary>
        /// Treeview's context menu > Expand.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmExpandSelected_Click(object sender, EventArgs e)
        {
            treeView.SelectedNode.Expand();
        }

        /// <summary>
        /// Treeview's context menu > Collapse.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmCollapseSelected_Click(object sender, EventArgs e)
        {
            treeView.SelectedNode.Collapse();
        }

        /// <summary>
        /// Treeview's context menu > Expand All.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmExpandAll_Click(object sender, EventArgs e)
        {
            treeView.ExpandAll();
            m_allExpanded = true;
        }

        /// <summary>
        /// Treeview's context menu > Collapse All.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmCollapseAll_Click(object sender, EventArgs e)
        {
            treeView.CollapseAll();
            m_allExpanded = false;
        }

        /// <summary>
        /// Context menu > Show "skill" in browser | Show "certificate class" certificates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showInBrowserMenu_Click(object sender, EventArgs e)
        {
            var skillTag = treeView.SelectedNode?.Tag as SkillLevel;

            if (skillTag != null)
                // Open the skill browser
                PlanWindow.ShowPlanWindow(m_character, m_plan).ShowSkillInBrowser(skillTag.
                    Skill);
        }

        /// <summary>
        /// Context menu > Show "skill" in explorer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showInExplorerMenu_Click(object sender, EventArgs e)
        {
            var skillTag = treeView.SelectedNode?.Tag as SkillLevel;

            if (skillTag != null)
                // Open the skill explorer
                SkillExplorerWindow.ShowSkillExplorerWindow(m_character, m_plan).
                    ShowSkillInExplorer(skillTag.Skill);
        }

        #endregion
    }
}
