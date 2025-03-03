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

CREATE TABLE Products (
	id_prod UNIQUEIDENTIFIER PRIMARY KEY,
	brand NVARCHAR (50) NOT NULL,
	gender NVARCHAR(1) NOT NULL,
	nome NVARCHAR (255) NOT NULL,
	id_size INT NOT NULL,
    id_color INT NOT NULL,
	id_material INT NOT NULL,
	descr NVARCHAR (1000) NULL,
	price DECIMAL (10,2) NOT NULL,
	stock INT NOT NULL,
	CONSTRAINT CK_presso CHECK (price >0),
	CONSTRAINT CK_stock CHECK (stock >=0),
	CONSTRAINT CK_gender CHECK (gender IN ('F','M','U')),
	CONSTRAINT FK_Prod_Material FOREIGN KEY (id_material) REFERENCES Materials (id_material),
	CONSTRAINT FK_Prod_Size FOREIGN KEY (id_size) REFERENCES Sizes (id_size),
	CONSTRAINT FK_Prod_Color FOREIGN KEY (id_color) REFERENCES Colors (id_color),
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



