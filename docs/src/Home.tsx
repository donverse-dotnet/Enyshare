import { Link, Routes } from "react-router-dom";
import "./index.css";

import logo2 from "./logo.png";
import comLogo from "./ggg-alpha.png";

export function Home() {
    return (
        <div className="app">
            <div className="logo-container">
                <img src={logo2} alt="Pocco Logo" className="logo bun-logo" style={{ borderRadius: "50%", height: "250px", width: "250px" }} />
                <img src={comLogo} alt="Community Logo" className="logo bun-logo" style={{ borderRadius: "50%", height: "250px", width: "250px" }} />
            </div>

            <h1>Pocco made by ggg-alpha</h1>
            <p>
                Super modern communication platform.
            </p>
        </div>
    );
}

export default Home;
