# Outpost 13

Ejercicio de clase de CC2008 POO, unidad 13 (interfaces). Unity 6.3 (URP) y C#.

## La misión

Eres la ingeniera de guardia del Outpost 13. Tienes 3 minutos de oxígeno para cargar el generador con 100% de
combustible y enviar la señal de rescate desde la consola de comunicaciones.

- En la bodega hay tres celdas de combustible: 40% en el piso, 35% detrás del crecimiento alienígena y 30% dentro de la
  caja de suministros.
- El generador y la consola de comunicaciones están en la sala norte, detrás de una puerta blindada que necesita la
  palanca.
- Empiezas con una pistola en la mano. En la sala norte hay una escopeta en el piso, pero todavía no sirve: es el
  ticket T7.
- Pierdes si el oxígeno llega a 0.

Al inicio el juego **no se puede ganar**: las celdas, el generador, la caja, los huevos, los barriles y la escopeta
están en la escena, pero no tienen código. Escribirlo es tu trabajo.

## Controles

| Entrada | Acción |
|---|---|
| clic izquierdo | caminar hasta ahí, o hasta el objeto que tocaste |
| E | usar lo que tienes al lado: puerta, palanca, consola y, desde T7, la escopeta |
| clic derecho | disparar el arma que tienes en la mano, una vez por clic |
| 1 / 2 | cambiar de arma, cuando ya tienes dos |
| R | reiniciar, cuando termina la partida |

## Antes de empezar

1. Clona el repositorio con `git clone`.
2. En Unity Hub, usa **Add › Add project from disk**, elige la carpeta clonada y ábrela con la versión **6000.3.8f1**. La
   primera vez tarda varios minutos: Unity construye la carpeta `Library`.
3. Abre la escena `Assets/Scenes/Outpost13.unity`, presiona Play y juega un minuto.
4. Abre Window › General › Test Runner, pestaña **PlayMode**, y presiona **Run All**. Las 7 pruebas deben pasar.
5. Haz un commit después de cada ticket. Si algo se rompe, `git diff` muestra qué cambiaste desde el último.

## El problema

`PlayerInteractor` y `PlayerShooter` funcionan, pero están mal diseñados: deciden qué hacer con cadenas de `if` sobre
el *tag* de cada objeto.

```csharp
if (c.CompareTag("Door")) c.GetComponentInParent<Door>().Open();
else if (c.CompareTag("Lever")) c.GetComponent<Lever>().Pull();
else if (c.CompareTag("Terminal")) c.GetComponent<Terminal>().Use(player);
```

Cada tipo de objeto nuevo obligaría a editar estos archivos. Es el mismo problema del `Checkout` con un `if` por método
de pago (unidad 12), y se arregla igual: con interfaces.

## La regla

Después del ticket T0, `PlayerInteractor.cs` **no se vuelve a tocar**, y `PlayerShooter.cs` solo una vez más, en T6.
Cada ticket es una clase nueva en `Assets/Scripts/Tickets/`, y las 7 pruebas deben seguir pasando después de cada uno.

## Tickets

### T0 · Refactorizar a interfaces

Crea las dos interfaces en `Assets/Scripts/Tickets/`. Las firmas son el contrato: deben quedar exactamente así.

```csharp
public interface IInteractable
{
    string Prompt();                // the label over the object, e.g. "Open door"
    void Interact(Player player);   // what happens when the engineer presses E
}

public interface IDamageable
{
    void TakeDamage(int amount);
}
```

1. `Door`, `Lever` y `Terminal` implementan `IInteractable`, y `AlienGrowth` implementa `IDamageable`. Los métodos
   nuevos solo llaman a lo que la clase ya tenía: `Open()`, `Pull()`, `Use(player)`, `Burn(amount)`.
2. En `PlayerInteractor`, busca el objeto más cercano con `GetComponentInParent<IInteractable>()` en vez de mirar
   tags. La etiqueta es `Prompt()`, y la tecla E llama a `Interact(player)`. Borra `IsUsable`, `PromptFor` y `Use`.
3. En `PlayerShooter`, en vez del `if` por tag, pídele `IDamageable` al objeto que golpeó el disparo y hazle el daño
   del arma (`gun.Damage`). No cambies el resto de `PlayerShooter`: eso es el ticket T6.

**Listo cuando** ninguno de los dos archivos contiene `CompareTag`, las 7 pruebas pasan y el juego se comporta igual
que antes.

### T1 · FuelCell

`FuelCell : MonoBehaviour, IInteractable`

- Un campo `[SerializeField] float charge`.
- `Prompt()` devuelve, por ejemplo, `"Pick up fuel cell (40%)"`.
- `Interact(player)` agrega `new Item("Fuel cell", charge)` a `player.Inventory` y destruye la celda.
- Agrega el componente a las tres celdas y ponle a cada una su carga: `FuelCell (40%)`, `FuelCell (35%)` y
  `FuelCell (30%)`. La última está escondida e inactiva dentro de `SupplyCrate`: despliégala en la jerarquía.

**Listo cuando** al tomar la celda suelta desaparece, y el HUD muestra `- Fuel cell (40%)` y `Total charge: 40%`.

### T2 · Generator

`Generator : MonoBehaviour, IInteractable`, en el objeto `Generator` de la sala norte.

- Un campo `[SerializeField] Terminal comms`: arrastra `CommsTerminal` ahí en el Inspector.
- `Interact(player)` toma todas las celdas con `player.Inventory.TakeAll()` y suma su carga. Al llegar a 100% o más,
  llama a `comms.PowerOn()`.
- `Prompt()` muestra la carga acumulada.

**Listo cuando** con las tres celdas (105%) la consola de comunicaciones se enciende, y E sobre ella gana el juego.

### T3 · SupplyCrate: una clase, dos interfaces

`SupplyCrate : MonoBehaviour, IInteractable, IDamageable`

- Se abre con E, o a disparos: tiene 30 de vida.
- Al abrirse, libera la celda escondida: sácala de la caja con `SetParent`, actívala con `SetActive(true)` y destruye
  la caja.
- Pista: `GetComponentInChildren<FuelCell>(true)` también encuentra hijos inactivos.

**Listo cuando** E abre la caja y la celda aparece; en otra partida, dispararle también la abre. Con T1, T2 y T3 el
juego ya se puede ganar.

### T4 · AlienEgg, y la ingeniera se vuelve IDamageable

1. `Player` (en `Scripts/Core`) implementa `IDamageable`: cada punto de daño quita un segundo de oxígeno con
   `Oxygen.Drain`. Es el único archivo de `Core` que se toca.
2. `AlienEgg : MonoBehaviour, IDamageable`, con 20 de vida. Cada 1.5 s suelta esporas: si la ingeniera está a 2 m o
   menos, recibe 3 de daño. Las esporas solo dañan a la ingeniera, no a otros objetos.
3. Agrégalo a los 8 huevos de una vez: escribe AlienEgg en el buscador de la jerarquía, selecciónalos todos y usa
   Add Component.

**Listo cuando** junto a un huevo el oxígeno baja más rápido, y dos disparos de pistola lo matan.

### T5 · ExplosiveBarrel y la reacción en cadena

`ExplosiveBarrel : MonoBehaviour, IDamageable`, con 10 de vida. Al llegar a 0 explota: hace 25 de daño a todo lo que
sea `IDamageable` a 3 m o menos (`Physics.OverlapSphere` y `GetComponentInParent<IDamageable>()`), y se destruye.

- Cuidado: un barril que explota daña a los otros, y esos lo dañan a él. Cada barril debe explotar una sola vez.

**Listo cuando** un disparo de pistola a cualquier barril de la sala norte hace explotar los tres y elimina los 5 huevos
del nido.
Si estás a 3 m o menos de la explosión, pierdes 25 s de oxígeno.

### T6 · Gun: una clase abstracta

Hoy todas las armas disparan igual: `PlayerShooter` lanza un solo rayo desde la boca del cañón. La escopeta de T7
dispara seis perdigones, y con este diseño habría que meter otro `if` en `PlayerShooter`. Mejor que cada arma sepa
disparar a su manera.

```csharp
public abstract class Gun : MonoBehaviour, IInteractable
{
    public abstract string Name { get; }
    protected abstract float SecondsBetweenShots { get; }
    protected abstract void Fire(Vector3 target);       // each gun fires its own way

    public void Trigger(Vector3 target) { ... }          // the same for every gun: cooldown, then Fire
    protected void ShootRay(Vector3 direction, int damage, float range) { ... }
    protected Vector3 DirectionTo(Vector3 target) { ... }
    // ...and what Gun already has: floating, PickUp, IsHeld
}
```

1. Haz `Gun` abstracta, con los tres miembros abstractos de arriba. Borra sus campos `gunName`, `damage`,
   `secondsBetweenShots` y `range`: ahora los define cada subclase.
2. `Trigger(target)` espera el tiempo entre disparos y llama a `Fire(target)`. `ShootRay` es el `Shoot` que hoy está
   en `PlayerShooter` (raycast, `IDamageable`, `Fx.Laser`, `Fx.Spark`), con la dirección y el daño como parámetros.
3. `Gun` implementa `IInteractable`: `Prompt()` devuelve `"Pick up " + Name.ToLower()` e `Interact(player)` llama a
   `PickUp(player)`. Toda arma se podrá recoger con E sin escribir nada más.
4. Crea `Handgun : Gun`: se llama `"Handgun"`, espera 0.35 s entre disparos, y su `Fire` hace un `ShootRay` hacia el
   objetivo con 10 de daño y 25 m de alcance.
5. `PlayerShooter` queda corto: con el clic derecho se voltea hacia el objetivo y llama a `gun.Trigger(punto)`. Borra
   `Shoot` y `nextShot`.
6. En la escena, `Player/Handgun` todavía tiene el componente `Gun`, que ya es abstracta: Unity no puede crear un
   objeto de una clase abstracta. Quítale ese componente y agrégale `Handgun`.

Una regla para decidir: lo que `Gun` necesita saber de cada arma (`Name`, `SecondsBetweenShots`, `Fire`) es abstracto;
lo que solo usa la subclase (el daño, el alcance) son campos de la subclase.

**Listo cuando** `PlayerShooter` no menciona daño, alcance ni rayos, la pistola funciona igual que antes y las 7
pruebas pasan.

### T7 · Shotgun

`Shotgun : Gun`, en el objeto `Shotgun` que está en el piso de la sala norte.

- Se llama `"Shotgun"` y espera 0.9 s entre disparos.
- Su `Fire` dispara 6 perdigones de 4 de daño y 9 m de alcance, cada uno girado un ángulo al azar entre -12° y 12°:
  `Quaternion.AngleAxis(angle, Vector3.up) * DirectionTo(target)`.
- No toques ningún otro archivo. Al agregar el componente, la escopeta flota, brilla y se recoge con E: todo eso lo
  hereda de `Gun`.

**Listo cuando** E recoge la escopeta, 1 y 2 cambian de arma, y dos disparos de escopeta queman el crecimiento
alienígena (la pistola necesita cuatro).

## Retos extra

- **S1 · IActivatable:** una palanca que puede encender cualquier cosa (una puerta, una consola) con `void Activate()`.
  Ojo: el Inspector no muestra campos de tipo interfaz; guarda un `GameObject` y pídele la interfaz con
  `GetComponent<IActivatable>()`.
- **S2 · IComparable\<Item\>:** el orden natural de los ítems es por carga. Es el `Comparable` de Java; necesita
  `using System;`.
- **S3 · IEnumerable\<Item\>:** que `foreach (Item item in player.Inventory)` funcione. Es el `Iterable` de Java; C#
  pide además un `GetEnumerator()` no genérico.
- Con S2 y S3, haz que el HUD liste las celdas de mayor a menor carga.
- **S4 · Pregunta:** `Door.Slide` devuelve `IEnumerator`. ¿Quién llama a su `MoveNext()`, y cada cuánto?

## Lo que ya existe y puedes usar

| Código | Qué hace |
|---|---|
| `player.Inventory.Add(item)` | agrega un ítem y lo anuncia en pantalla |
| `player.Inventory.TakeAll()` | entrega todos los ítems y deja el inventario vacío |
| `player.Inventory.TotalCharge()` | suma la carga de todo lo que lleva |
| `player.Oxygen.Drain(segundos)` | quita oxígeno |
| `player.Loadout.Current` | el arma que tiene en la mano, un `Gun` |
| `Hud.Instance.Toast("texto")` | muestra un mensaje corto abajo de la pantalla |
| `Fx.Explosion(posición, escala)` | dibuja una explosión |
| `comms.PowerOn()` | le da energía a una `Terminal` |
| `GetComponentInParent<T>()` | busca el componente `T` en el objeto o en sus padres |
| `GetComponentInChildren<T>(true)` | busca en los hijos, incluidos los inactivos |
| `Physics.OverlapSphere(centro, radio)` | todos los colliders dentro de una esfera |
| `Destroy(gameObject)` | elimina el objeto al terminar el frame |

El texto que se ve en el juego debe ser ASCII simple, sin tildes ni ñ, porque la fuente del juego no las tiene.
Escribe los mensajes en inglés, como el resto del juego.

## Estructura del proyecto

| Carpeta | Qué hay |
|---|---|
| `Assets/Scenes/` | la escena del juego |
| `Assets/Scripts/Core/` | jugadora, cámara, movimiento, oxígeno, HUD y efectos. No hace falta editarlos (salvo `Player` en T4) |
| `Assets/Scripts/World/` | puerta, palanca, consolas, `Gun` y crecimiento alienígena |
| `Assets/Scripts/Player/` | `PlayerInteractor` y `PlayerShooter`: se arreglan en T0 (`PlayerShooter`, otra vez en T6) |
| `Assets/Scripts/Tickets/` | tus interfaces y clases nuevas |
| `Assets/Tests/PlayMode/` | las pruebas que juegan la escena |

## Errores comunes

| Lo que ves | Causa |
|---|---|
| `CS0737`: ... cannot implement an interface member because it is not public | el método implementado no es `public` |
| `CS0535`: ... does not implement interface member ... | falta un método, o su firma es distinta a la de la interfaz |
| `CS1722`: Base class 'MonoBehaviour' must come before any interfaces | el orden correcto es `: MonoBehaviour, IInteractable` |
| `CS0246`: The type or namespace name ... could not be found | nombre mal escrito, o falta `using System;` (S2) |
| `CS0534`: ... does not implement inherited abstract member ... | a la subclase le falta un `override` de `Gun` |
| `CS0506`: ... cannot override inherited member ... because it is not marked virtual, abstract, or override | en C# solo se sobrescribe lo que la clase base marca `abstract` o `virtual` |
| Unity no deja agregar el script a un objeto | el archivo no se llama igual que la clase, o hay un error de compilación |
| La puerta ya no muestra su etiqueta | usaste `GetComponent`: el collider de la puerta está en su hijo `Panel` |
| El clic derecho no dispara y arriba a la derecha dice `NONE` | `Gun` ya es abstracta y `Player/Handgun` todavía tiene el componente `Gun`: cámbialo por `Handgun` (T6) |
| *The class named 'Gun' is abstract* | lo mismo |
| Los clics y las teclas no hacen nada | haz clic una vez dentro de la vista Game |

## De Java a C#

| Java | C# |
|---|---|
| `class A extends B implements C` | `class A : B, IC` (la clase base primero) |
| `Comparable<T>`, `compareTo` | `IComparable<T>`, `CompareTo` |
| `Iterable<T>`, `iterator()` | `IEnumerable<T>`, `GetEnumerator()` |
| `Iterator<T>`: `hasNext()`, `next()` | `IEnumerator<T>`: `MoveNext()`, `Current` |
| `x instanceof Door d` | `x is Door d`; en Unity, `GetComponent<Door>()` |
| `class Handgun extends Gun` | `class Handgun : Gun` |
| `abstract String getName();` | `public abstract string Name { get; }` (propiedad abstracta) |
| `@Override`, opcional | `override`, obligatorio |
| todo método se puede sobrescribir | solo los `abstract` o `virtual` |
