CREATE DATABASE	ECommerceDB;

USE ECommerceDB;


CREATE TABLE Users(
	id_user UNIQUEIDENTIFIER PRIMARY KEY,
    email NVARCHAR(100) NOT NULL UNIQUE,
    user_role NVARCHAR(50)  NOT NULL,
	CONSTRAINT CK_ruolo CHECK (user_role in ('Seller','User') )
)

CREATE TABLE Sizes (
    id_size INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE,
);

CREATE TABLE Colors (
    id_color INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);


CREATE TABLE Materials (
    id_material INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Category (
    id_category INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Products (
	id_prod UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	brand NVARCHAR (50) NOT NULL,
	gender NVARCHAR(1) NOT NULL,
	nome NVARCHAR (255) NOT NULL,
	id_category INT NOT NULL,
	descr NVARCHAR (1000) NULL,
	price DECIMAL (10,2) NOT NULL,
	stock INT NOT NULL,
	CONSTRAINT CK_presso CHECK (price >0),
	CONSTRAINT CK_stock CHECK (stock >=0),
	CONSTRAINT CK_gender CHECK (gender IN ('F','M','U')),
	CONSTRAINT FK_Prod_Catogry FOREIGN KEY (id_category) REFERENCES Category (id_category),
)

CREATE TABLE ProdSize (
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_size INT NOT NULL,
	CONSTRAINT PK_ProdSize PRIMARY KEY (id_prod, id_size),
	CONSTRAINT FK_ProdSize_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdSize_size FOREIGN KEY (id_size) REFERENCES Sizes (id_size),
)

CREATE TABLE ProdColor (
    id_prodColor UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE,
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_color INT NOT NULL,
	CONSTRAINT PK_ProdColor PRIMARY KEY (id_prod, id_color),
	CONSTRAINT FK_ProdColor_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdColor_color FOREIGN KEY (id_color) REFERENCES Colors (id_color),
)

CREATE TABLE ProdColorImages (
    id_prodColorImage UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    id_prodColor UNIQUEIDENTIFIER NOT NULL,
    img_URL NVARCHAR(1000) NOT NULL,
    CONSTRAINT FK_ProdColorImages_prodColor FOREIGN KEY (id_prodColor) REFERENCES ProdColor(id_prodColor)
);


CREATE TABLE ProdMaterial (
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_material INT NOT NULL,
	percentage_mat DECIMAL (5,2) NOT NULL,
	CONSTRAINT PK_ProdMat PRIMARY KEY (id_prod, id_material),
	CONSTRAINT CK_percentage CHECK (percentage_mat BETWEEN 0 AND 100),
	CONSTRAINT FK_ProdMat_prod FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_ProdMat_Mat FOREIGN KEY (id_material) REFERENCES Materials (id_material),
)


CREATE TABLE Orders (
	id_order INT IDENTITY (1,1) PRIMARY KEY,
	id_user UNIQUEIDENTIFIER NOT NULL,
	date_order DATETIME DEFAULT GETDATE(),
	total DECIMAL(10,2) not null,
	stateOrder NVARCHAR(50) NOT NULL,
	CONSTRAINT CK_totale CHECK (total >= 0),
	CONSTRAINT CK_stato CHECK (stateOrder IN ('in elaborazione','spedito','consegnato')),
	CONSTRAINT FK_Ordini_Utenti FOREIGN KEY (id_user) REFERENCES Users (id_user),
)

CREATE TABLE Cart (
	id_order INT IDENTITY (1,1) NOT NULL,
	id_prod UNIQUEIDENTIFIER NOT NULL,
	qt INT NOT NULL,
	price DECIMAL (10,2) NOT NULL,
	CONSTRAINT PK_ordineprodotto PRIMARY KEY (id_order, id_prod), 
	CONSTRAINT CK_quantita CHECK (qt >0),
	CONSTRAINT CK_prezzo_unitario CHECK (price >0),
	CONSTRAINT FK_OrdineProdotto_Ordine FOREIGN KEY (id_order) REFERENCES Orders (id_order),
	CONSTRAINT FK_OrdineProdotto_Prodotto FOREIGN KEY (id_prod) REFERENCES Products (id_prod) 
)

CREATE TABLE Ratings(
	id_rating UNIQUEIDENTIFIER PRIMARY KEY,
	id_prod UNIQUEIDENTIFIER NOT NULL,
	id_user UNIQUEIDENTIFIER NOT NULL,
	comment NVARCHAR(500), 
	rating INT NOT NULL,
	CreatedAt DATETIME DEFAULT GETDATE(),
	CONSTRAINT CK_rating CHECK (rating BETWEEN 1 AND 5),
	CONSTRAINT FK_Rating_Prodotto FOREIGN KEY (id_prod) REFERENCES Products (id_prod),
	CONSTRAINT FK_Rating_Users FOREIGN KEY (id_user) REFERENCES Users (id_user), 
)



