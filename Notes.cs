//Add notification click events


//Button stuff
private void Button_Click(object sender, EventArgs e)
{
    // Determine which button was clicked
    if (sender == CloudButton)
    {
        StartCloud();
        isCloud = true;
        isSpring = false;
        isSummer = false;
        isFall = false;
        isSnow = false;
    }
    else if (sender == SummerButton)
    {
        StartSummer();
        isCloud = false;
        isSpring = false;
        isSummer = true;
        isFall = false;
        isSnow = false;
    }
    else if (sender == SpringButton)
    {
        StartSpring();
        isCloud = false;
        isSpring = true;
        isSummer = false;
        isFall = false;
        isSnow = false;
    }
    else if (sender == FallButton)
    {
        StartFall();
        isCloud = false;
        isSpring = false;
        isSummer = false;
        isFall = true;
        isSnow = false;
    }
    else if (sender == SnowButton)
    {
        StartSnow();
        isCloud = false;
        isSpring = false;
        isSummer = false;
        isFall = false;
        isSnow = true;
    }

    // Update the UI
    MainPictureBox.Invalidate();
    SkinsButton.Visible = true;
    SkinsButton.Enabled = true;

    CancelButton.Visible = false;
    SetButtonVisibility(false);
}

private void Form1_Load(object sender, EventArgs e)
{
    // Attach the Button_Click event handler to all the buttons
    CloudButton.Click += Button_Click;
    SummerButton.Click += Button_Click;
    SpringButton.Click += Button_Click;
    FallButton.Click += Button_Click;
    SnowButton.Click += Button_Click;

}
