using UnityEngine;

public class DisplayInventory : MonoBehaviour {

    [SerializeField] private float ButtonSize;
    [SerializeField] private float ButtonGap;
    [SerializeField] private float ZLayer;
    [SerializeField] private Camera GameplayCamera;
    [SerializeField] private Font CounterFont;
    [SerializeField] private Material CounterMaterial;
    [SerializeField] private float CounterRelativeSize;
    [SerializeField] private Vector3 CounterRelativePosition;

    private static DisplayInventory mInstance;
    private Vector3[] ButtonPositions;
    private GameObject[] Buttons;
    private bool ScreenHorizontal;
    private float ButtonSizeInPixels;
    private Vector2 ScreenCornerInPixels;

    //Through delegate, PlayerCharacter will allow this class to acces Inventory array
    public delegate int[] GetInventory();
    public static event GetInventory GetInventoryDelegate;

    void Awake() {
        if (mInstance == null) {
            mInstance = this;
        }
        else {
            Debug.LogError("Only one DisplayInventory allowed! Destorying duplicate.");
            Destroy(this.gameObject);
        }
    }
        void Start () {
        
        // Work out where the corner button should be
        float y = GameLogic.ScreenHeight * 0.5f;
        float x = y * GameplayCamera.aspect;
        //ScreenCorner = new Vector2(x, y);

        //Work out some pixel sizes which will be needed when user taps on the inventory
        ScreenCornerInPixels = new Vector2(Screen.width, Screen.height);
        ButtonSizeInPixels = ScreenCornerInPixels.y * (ButtonSize + ButtonGap) / GameLogic.ScreenHeight;

        //Work out centre of cornermost button
        x -= ButtonGap + ButtonSize * 0.5f;
        y -= ButtonGap + ButtonSize * 0.5f;

        //Create positions for the buttons
        ButtonPositions = new Vector3[(int)PowerupFactory.Type.NoPowerups];
        ButtonPositions[0] = new Vector3(x, y, ZLayer);

        if (GameplayCamera.aspect < 1)
        { //Tall screen, place buttons vertically
            ScreenHorizontal = false;
            for (int i = 1; i < (int)PowerupFactory.Type.NoPowerups; i++) {
                y -= ButtonGap + ButtonSize;
                ButtonPositions[i] = new Vector3(x, y, ZLayer);
            }
        }
        else
        { //Wide screen, place buttons horizontally
            ScreenHorizontal = true;
            for (int i = 1; i < (int)PowerupFactory.Type.NoPowerups; i++) {
                x -= ButtonGap + ButtonSize;
                ButtonPositions[i] = new Vector3(x, y, ZLayer);
            }
        }

        //Get materials from powerup factory
        Material[] Materials = PowerupFactory.mInstance.mMaterials;

        //Create buttons
        Buttons = new GameObject[(int)PowerupFactory.Type.NoPowerups];
        for (int i=0; i<(int)PowerupFactory.Type.NoPowerups; i++) {
            Buttons[i] = new GameObject("Button_Pool_ID_"+(i+1));

            //ZGive buttons a shape
            CreateMesh2 Mesh = Buttons[i].AddComponent<CreateMesh2>();
            Mesh.Material = Materials[i];

            //Attach a text field to buttons
            GameObject counter = new GameObject("Counter_of_Button_" + (i + 1));
            counter.transform.parent = Buttons[i].transform;

            //Give text a position and size
            counter.transform.localScale = new Vector3(CounterRelativeSize, CounterRelativeSize, CounterRelativeSize);
            counter.transform.position = CounterRelativePosition;

            //Give text some... text.
            TextMesh counterMesh = counter.AddComponent<TextMesh>();

            //Give text a shape
            counterMesh.font = CounterFont;
            MeshRenderer meshRenderer = counter.GetComponent<MeshRenderer>();
            meshRenderer.material = CounterMaterial;

            //Give buttons a position and scale
            Buttons[i].transform.localScale = new Vector3(ButtonSize, ButtonSize, ButtonSize);
            Buttons[i].transform.position = ButtonPositions[i];

            //Hide buttons for now (when something appears in the inventory, they will be made active)
            Buttons[i].SetActive(false);
        }

	}
	
	void Update () {
        int[] Inventory = GetInventoryDelegate();

        for (int i = 0; i < (int)PowerupFactory.Type.NoPowerups; i++) {
            if (Inventory[i] > 0) {

                //If item exists in inventory, show a button for it and display the amount
                Buttons[i].SetActive(true);
                Buttons[i].GetComponentInChildren<TextMesh>().text = "" + Inventory[i];
            }
            else {

                //If item doesn't exist, hide its button
                Buttons[i].SetActive(false);
            }
        }
	}

    /*This is called when the user taps on the screen, to decide if an inventory button was tapped and
       a special bullet should be shot.
    */
    public static int ButtonAt(Vector2 position) {
        if (mInstance == null) return -1;
        else return mInstance.ButtonAtOnInstance(position);
    }

    private int ButtonAtOnInstance (Vector2 position) {
        int narrow, wide;
        position = (ScreenCornerInPixels - position)/ButtonSizeInPixels;
        if (ScreenHorizontal) {
            narrow = (int)(position.y);
            wide = (int)(position.x);
        }
        else {
            narrow = (int)(position.x);
            wide = (int)(position.y);
        }
        if (narrow==0 && wide>=0 && wide<(int)PowerupFactory.Type.NoPowerups && Buttons[wide].activeInHierarchy) {
            return wide;
        }
        return -1;
    }
}
