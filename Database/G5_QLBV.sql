
USE G5_QLBV;

CREATE TABLE USERS(
	UserId int,
	Username varchar(50),
	PasswordHash varchar(255),
	Fullname nvarchar(100),
	[Role] varchar(20),
	IsActive bit,
	CONSTRAINT PK_USERS PRIMARY KEY (UserId)
);

CREATE TABLE PATIENTS(
	PatientId int,
	Fullname varchar(100),
	PasswordHash varchar(255),
	Phone varchar(15),
	DateofBirth smalldatetime,
	Gender nvarchar(10),
	HealthInsuranceCode varchar(20),
	CONSTRAINT PK_PATIENTS PRIMARY KEY (PatientId)
);

CREATE TABLE ROOMS(
	RoomID int,
	RoomName nvarchar(50),
	[Status] nvarchar(20),
	CONSTRAINT PK_ROOMS PRIMARY KEY (RoomID)
);

CREATE TABLE SERVICES(
	ServiceID int,
	ServiceName nvarchar(100),
	Price decimal,
	category nvarchar(50),
	CONSTRAINT PK_SERVICES PRIMARY KEY (ServiceID)
);

CREATE TABLE DRUGS(
	DrugID int,
	DrugName nvarchar(100),
	Unit nvarchar(20),
	UnitPrice decimal,
	StockQuantity int,
	CONSTRAINT PK_DRUGS PRIMARY KEY (DrugID)
);

CREATE TABLE SHIFTS(
	ShiftID int,
	UserID int,
	RoomID int,
	ShiftDate smalldatetime,
	TimeFrame varchar(20),
	CONSTRAINT PK_SHIFTS PRIMARY KEY (ShiftID),
	CONSTRAINT FK_SHIFTS_USERS FOREIGN KEY (UserID) REFERENCES USERS,
	CONSTRAINT FK_SHIFTS_ROOMS FOREIGN KEY (RoomID) REFERENCES ROOMS
);

CREATE TABLE APPOINTMENTS(
	ApptID int,
	PatientID int,
	DoctorID int,
	ApptDate date,
	TimeSlot varchar(20),
	[Status] nvarchar(20),
	CONSTRAINT PK_APPOINTMENTS PRIMARY KEY (ApptID),
	CONSTRAINT FK_APPOINTMENTS_PATIENTS FOREIGN KEY (PatientID) REFERENCES PATIENTS,
	CONSTRAINT FK_APPOINTMENTS_USERS FOREIGN KEY (DoctorID) REFERENCES USERS(UserID)
);

CREATE TABLE MEDICALRECORDS(
	RecordID int,
	PatientID int,
	NurseID int,
	DoctorID int,
	VisitDate smalldatetime,
	Symptoms nvarchar(1000),
	Diagnosis nvarchar(1000),
	IsInpatient bit DEFAULT 0,
	[Status] nvarchar(20),
	CONSTRAINT PK_MEDICALRECORDS PRIMARY KEY (RecordID),
	CONSTRAINT FK_MEDICALRECORDS_PATIENTS FOREIGN KEY (PatientID) REFERENCES PATIENTS,
	CONSTRAINT FK_MEDICALRECORDS_USERS_1 FOREIGN KEY (DoctorID) REFERENCES USERS(UserID),
	CONSTRAINT FK_MEDICALRECORDS_USERS_2 FOREIGN KEY (NurseID) REFERENCES USERS(UserID)
);

CREATE TABLE EXAMMETRICS (
    RecordID int,
    VisionRight float,                 -- Thị lực mắt phải
    VisionLeft float,                  -- Thị lực mắt trái
    IOP_Right float,                   -- Nhãn áp mắt phải
    IOP_Left float,                    -- Nhãn áp mắt trái
    -- Chỉ số Khúc xạ (Đơn kính chuyên khoa)
    SPH_Right float,                   -- Độ Cầu (Sphere) mắt phải
    CYL_Right float,                   -- Độ Trụ (Cylinder) mắt phải
    AXIS_Right float,                  -- Trục (Axis) mắt phải
    SPH_Left float,                    -- Độ Cầu mắt trái
    CYL_Left float,                    -- Độ Trụ mắt trái
    AXIS_Left float,                   -- Trục mắt trái
	CONSTRAINT PK_EXAMMETRICS PRIMARY KEY (RecordID),
    CONSTRAINT FK_EXAMMETRICS_MEDICALRECORDS FOREIGN KEY (RecordID) 
    REFERENCES MEDICALRECORDS(RecordID) ON DELETE CASCADE
);
CREATE TABLE BILLS(
	BillID int,
	--PatientID int,
	RecordID int,
	TotalAmount decimal,
	CreatedAt smalldatetime,
	BillType nvarchar(30),
	[Status] nvarchar(20),
	CONSTRAINT PK_BILLS PRIMARY KEY (BillID),
	--CONSTRAINT FK_BILLS_PATIENTS FOREIGN KEY (PatientID) REFERENCES PATIENTS,
	CONSTRAINT FK_BILLS_MEDICALRECORDS FOREIGN KEY (RecordID) REFERENCES MEDICALRECORDS
);

CREATE TABLE PRESCRIPTIONDETAILS(
	RecordID int,
	DrugID int,
	Quantity int,
	UnitPrice decimal(18,2),
	Dosage nvarchar(255),
	CONSTRAINT PK_PRESCRIPTIONDETAILS PRIMARY KEY (RecordID, DrugID),
	CONSTRAINT FK_PRESCRIPTIONDETAILS_MEDICALRECORDS FOREIGN KEY (RecordID) REFERENCES MEDICALRECORDS(RecordID),
	CONSTRAINT FK_PRESCRIPTIONDETAILS_DRUGS  FOREIGN KEY (DrugID) REFERENCES DRUGS(DrugID)
);

CREATE TABLE SERVICEDETAILS(
	RecordID int,
	ServiceID int,
	ResultNote nvarchar(1000),
	UnitPrice decimal(18,2),
	PaymentStatus nvarchar(20),
	CONSTRAINT PK_SERVICEDETAILS PRIMARY KEY (RecordID, ServiceID),
	CONSTRAINT FK_SERVICEDETAILS_MEDICALRECORDS FOREIGN KEY (RecordID) REFERENCES MEDICALRECORDS,
	CONSTRAINT FK_SERVICEDETAILS_SERVICES FOREIGN KEY (ServiceID) REFERENCES SERVICES
);

CREATE TABLE CAREORDERS(
	CareOrderID int,
	RecordID int,
	NurseID int,
	ActionName nvarchar(255),
	[Status] nvarchar(20),
	ExecuteAt smalldatetime,
	CONSTRAINT PK_CAREORDERS PRIMARY KEY (CareOrderID),
	CONSTRAINT FK_CAREORDERS_MEDICALRECORDS FOREIGN KEY (RecordID) REFERENCES MEDICALRECORDS(RecordID),
	CONSTRAINT FK_CAREORDERS_USERS FOREIGN KEY (NurseID) REFERENCES USERS(UserID)
);

CREATE TABLE ARTICLES(
	ArticleID int,
	Title nvarchar(200),
	Content nvarchar(max),
	Category nvarchar(50),
	AuthorID int,
	CreatedAt smalldatetime,
	CONSTRAINT PK_ARTICLES PRIMARY KEY (ArticleID),
	CONSTRAINT FK_ARTICLES_USERS FOREIGN KEY (AuthorID) REFERENCES USERS(UserID)
);

CREATE TABLE QUESTIONS(
	QuestionID int,
	PatientID int,
	DoctorID int,
	Content nvarchar(max),
	AnswerContent nvarchar(max),
	[Status] nvarchar(20),
	CreatedAt smalldatetime,
	CONSTRAINT PK_QUESTIONS PRIMARY KEY (QuestionID),
	CONSTRAINT FK_QUESTIONS_PATIENTS FOREIGN KEY (PatientID) REFERENCES PATIENTS(PatientId),
	CONSTRAINT FK_QUESTIONS_USERS FOREIGN KEY (DoctorID) REFERENCES USERS(UserID)
);
CREATE TABLE SYSLOGS(
	LogID int,
	UserID int,
	Action nvarchar(500),
	LoggedAt smalldatetime,
	CONSTRAINT PK_SYSLOGS PRIMARY KEY (LogID),
	CONSTRAINT FK_SYSLOGS_USERS FOREIGN KEY (UserID) REFERENCES USERS(UserID)
);