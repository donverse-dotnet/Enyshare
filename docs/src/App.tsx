import { Routes } from "react-router-dom";
import "./index.css";

import routes from "./Routes";

export function App() {
    return (
        <Routes>
            {routes}
        </Routes>
    );
}

export default App;
